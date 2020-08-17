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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.TemplateEngine.Models;
using Steeltoe.Initializr.TemplateEngine.Services.Mustache.Expressions;
using Steeltoe.Initializr.TemplateEngine.Utilities;

namespace Steeltoe.Initializr.TemplateEngine.Services.Mustache
{
    public class MustacheConfig
    {
        private readonly ILogger _logger;
        private readonly IDictionary<TemplateKey, MustacheTemplateSettings> _templateSettings;

        public MustacheConfig(ILogger logger, string templatePath)
        {
            _logger = logger;
            _templateSettings = new Dictionary<TemplateKey, MustacheTemplateSettings>();
            if (!string.IsNullOrEmpty(templatePath))
            {
                LoadConfig(templatePath);
            }
        }

        public MustacheConfigSchema GetSchema(TemplateKey templateKey)
        {
            return _templateSettings[templateKey].Schema;
        }

        public IEnumerable<TemplateKey> GetTemplateKeys()
        {
            return _templateSettings.Keys;
        }

        public async Task<Dictionary<string, string>> GetDataView(TemplateKey templateKey, GeneratorModel model)
        {
            var settings = _templateSettings[templateKey];
            var mustacheConfig = settings.Schema;

            var dataView = new Dictionary<string, string>();
            using (Timing.Over(_logger, "GetDataView-Rest"))
            {
                foreach (var dep in mustacheConfig.Params)
                {
                    dataView.Add(dep.Name, dep.DefaultValue);
                }

                var dependencies = model.GetDependencies();
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
                    else
                    {
                        throw new InvalidDataException("SteeltoeVersion: InvalidParamValues");
                    }
                }

                if (model.TargetFramework != null)
                {
                    const string targetFrameworkName = "TargetFrameworkVersion";
                    var targetFramework = mustacheConfig.Versions.FirstOrDefault(v => v.Name == targetFrameworkName);

                    var validChoice =
                        targetFramework?.Choices.Any(choice => model.TargetFramework.ToLower() == choice.Choice);

                    if (validChoice == true)
                    {
                        dataView[targetFrameworkName] = model.TargetFramework;
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
                foreach (var (name, expression) in settings.EvaluationExpressions)
                {
                    var result = await expression.EvaluateExpressionAsync(dataView);
                    dataView.Add(name, result);
                }
            }

            return dataView;
        }

        public IEnumerable<SourceFile> GetFilteredSourceSets(Dictionary<string, string> dataView,
            TemplateKey templateKey)
        {
            var settings = _templateSettings[templateKey];
            var files = settings.SourceSets;
            var exclusionExpressions = GetInclusionExpressions(dataView, settings.Schema);
            var excludedFiles = new List<string>();
            foreach (var sourceFile in files)
            {
                if (sourceFile.Name.EndsWith("mustache.json"))
                {
                    excludedFiles.Add(sourceFile.Name);
                    continue;
                }

                // By default everything is included, unless there is a conditional inclusions
                // which causes it to be an exclusion when condition is not met.
                // Explicit inclusions override exclusions by condition not being met
                var explicitInclusions = new List<string>();
                foreach (var exclusionExpression in exclusionExpressions)
                {
                    if (exclusionExpression.IsMatch(sourceFile.Name))
                    {
                        if (exclusionExpression.IsInclusion)
                        {
                            explicitInclusions.Add(sourceFile.Name);
                        }
                        else
                        {
                            excludedFiles.Add(sourceFile.Name);
                        }
                    }
                }

                excludedFiles = excludedFiles.Except(explicitInclusions).ToList();
            }

            return files.Where(f => excludedFiles.All(e => e != f.Name));
        }

        private void LoadConfig(string templatePath)
        {
            var frameworks = new[] {"netcoreapp2.1", "netcoreapp3.1"};
            foreach (var framework in frameworks)
            {
                var path = Path.Join(templatePath, "2.4", framework);
                foreach (var dir in new DirectoryInfo(path).EnumerateDirectories())
                {
                    var template = dir.Name;
                    var mustacheTemplateSetting = new MustacheTemplateSettings(_logger, dir.FullName);
                    _templateSettings.Add(new TemplateKey(framework, template), mustacheTemplateSetting);
                }
            }
        }

        private List<InclusionExpression> GetInclusionExpressions(Dictionary<string, string> dataView,
            MustacheConfigSchema schema) =>
            schema.ConditionalInclusions
                .Select(x => new InclusionExpression(
                    expression: x.InclusionExpression,
                    matchesView: dataView.ContainsKey(x.Name) &&
                                 (dataView[x.Name] is string stringValue && stringValue == "True")))
                .ToList();
    }
}
