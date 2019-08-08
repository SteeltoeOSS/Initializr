// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Steeltoe.Initializr.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.TemplateEngine.Core.Matching;
using Steeltoe.Initializr.Utilities;

namespace Steeltoe.Initializr.Services.Mustache
{
    public class MustacheConfig
    {
        private readonly ILogger _logger;
        private readonly IDictionary<string, MustacheConfigSchema> _templateConfigs;
        private readonly IDictionary<string, IEnumerable<SourceFile>> _sourceSets;
        private readonly IDictionary<string, IDictionary<string, IExpression>> _evaluationExpressions = new Dictionary<string, IDictionary<string, IExpression>>();

        public MustacheConfig(ILogger logger, string templatePath)
        {
            _logger = logger;
            _templateConfigs = new Dictionary<string, MustacheConfigSchema>();
            _sourceSets = new Dictionary<string, IEnumerable<SourceFile>>();
            if (!string.IsNullOrEmpty(templatePath))
            {
                LoadConfig(templatePath);
            }
        }

        private void LoadConfig(string templatePath)
        {
            foreach (var dir in new DirectoryInfo(templatePath).EnumerateDirectories())
            {
                var schema = ReadSchema(dir.FullName);
                _templateConfigs.Add(dir.Name, schema);

                _sourceSets.Add(dir.Name, GetSourceSets(templatePath + Path.DirectorySeparatorChar + dir.Name));

                var evalExpressions = new Dictionary<string, IExpression>();
                foreach (var calculatedParam in schema.CalculatedParams)
                {
                    IExpression expression = null;
                    switch (calculatedParam.ExpressionType)
                    {
                        case ExpressionTypeEnum.Any:
                            expression = new AnyExpression(_logger, calculatedParam, schema);
                            break;
                        case ExpressionTypeEnum.Bool:
                            expression = new BooleanExpression(_logger, calculatedParam, schema);
                            break;
                        case ExpressionTypeEnum.Case:
                            expression = new CaseExpression(_logger, calculatedParam, schema);
                            break;
                        case ExpressionTypeEnum.String:
                            expression = new CaseExpression(_logger, calculatedParam, schema);
                            break;
                    }

                    if (expression != null)
                    {
                        evalExpressions.Add(calculatedParam.Name, expression);
                    }
                }

                _evaluationExpressions.Add(dir.Name, evalExpressions);

            }
        }

        private IEnumerable<SourceFile> GetSourceSets(string path)
        {
            var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).ToList();
            var returnValue = new List<SourceFile>();
            foreach (var file in files)
            {
                returnValue.Add(new SourceFile {
                    Name = file.Replace(Path.GetFullPath(path), string.Empty)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    FullPath = file,
                    Text = File.ReadAllText(file),
                });
            }

            return returnValue;
        }

        private List<string> GetExclusionList(Dictionary<string, string> dataView, MustacheConfigSchema schema)
        {
            // This list of expressions that do NOT apply become the exclusion list here
            return schema.ConditionalInclusions.Where(x => !dataView.ContainsKey(x.Name)
                                                           || (dataView[x.Name] is string stringValue && stringValue != "True"))
                .Select(x => x.InclusionExpression).ToList();
        }

        public IEnumerable<SourceFile> GetFilteredSourceSets(Dictionary<string, string> dataView, string templateName)
        {
            var files = _sourceSets[templateName].ToList();
            var exclusionExpressions = GetExclusionList(dataView, _templateConfigs[templateName]);
            var excludedFiles = new List<string>();
            foreach (var sourceFile in files)
            {
                if (sourceFile.Name.EndsWith("mustache.json")
                    || sourceFile.Name.EndsWith("sourceExclusions.json"))
                {
                    continue;
                }

                foreach (var exclusionExpression in exclusionExpressions)
                {

                    if (exclusionExpression.EndsWith("**"))
                    {
                        if (sourceFile.Name.StartsWith(exclusionExpression.Replace("/**", string.Empty)))
                        {
                            excludedFiles.Add(sourceFile.Name);
                        }
                    }
                    else
                    {
                        // exact Match
                        if (sourceFile.Name == exclusionExpression)
                        {
                            excludedFiles.Add(sourceFile.Name);
                        }
                    }
                }
            }

            return files.Where(f => excludedFiles.All(e => e != f.Name));

        }

        private MustacheConfigSchema ReadSchema(string templatePath)
        {
            var json = File.ReadAllText(Path.Combine(templatePath, "mustache.json"));
            var returnValue = JsonConvert.DeserializeObject<MustacheConfigSchema>(json);
            if (returnValue == null)
            {
                throw new InvalidDataException($"could not find config at {templatePath}");
            }

            return returnValue;
        }

        public MustacheConfigSchema GetSchema(string templateName)
        {
            return _templateConfigs[templateName];
        }

        public IEnumerable<string> GetTemplateNames()
        {
            return _templateConfigs.Keys;
        }

        public async Task<Dictionary<string, string>> GetDataView(string templateName, string [] dependencies, GeneratorModel model)
        {
            var mustacheConfig = _templateConfigs[templateName];

            var dataView = new Dictionary<string, string>();
            using (Timing.Over(_logger, "GetDataView-Rest"))
            {
                foreach (var dep in mustacheConfig.Params)
                {
                    dataView.Add(dep.Name, dep.DefaultValue);
                }

                if (dependencies != null)
                {
                    foreach (var dependencyName in dependencies)
                    {
                        var key = mustacheConfig.Params.FirstOrDefault(k => k.Name.ToLower() == dependencyName)?.Name;
                        if (key != null)
                        {
                            dataView[key] = "True";
                        }
                    }
                }

                foreach (var version in mustacheConfig.Versions)
                {
                    dataView.Add(version.Name, version.DefaultValue);
                }

                if (model.SteeltoeVersion != null)
                {
                    const string steeltoeVersionName = "SteeltoeVersion";
                    var steeltoeVersion = mustacheConfig.Versions.FirstOrDefault(v => v.Name == steeltoeVersionName);
                    var validChoice =
                        steeltoeVersion?.Choices.Any(choice => model.SteeltoeVersion.ToLower() == choice.Choice);
                    if (validChoice == true)
                    {
                        dataView[steeltoeVersionName] = model.SteeltoeVersion.ToLower();
                    }
                }

                if (model.TargetFrameworkVersion != null)
                {
                    const string targetFrameworkName = "TargetFrameworkVersion";
                    var targetFramework = mustacheConfig.Versions.FirstOrDefault(v => v.Name == targetFrameworkName);

                    var validChoice =
                        targetFramework?.Choices.Any(choice => model.TargetFrameworkVersion.ToLower() == choice.Choice);

                    if (validChoice == true)
                    {
                        dataView[targetFrameworkName] = model.TargetFrameworkVersion;
                    }
                }

                if (model.ProjectName != null)
                {
                    const string projectNamespaceName = "ProjectNameSpace";
                    if (dataView.ContainsKey(projectNamespaceName))
                    {
                        dataView[projectNamespaceName] = model.ProjectName;
                    }
                }
            }

            using (Timing.Over(_logger, "GetDataView-CalculatedParams"))
            {
                foreach (var (name, expression) in _evaluationExpressions[templateName])
                {
                    var result = await expression.EvaluateExpressionAsync(dataView);
                    dataView.Add(name, result);
                }
            }

            return dataView;
        }
    }
}
