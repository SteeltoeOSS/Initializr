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

namespace Steeltoe.Initializr.Services.Mustache
{
    public class MustacheConfig
    {
        public IEnumerable<string> GetFilteredSourceSets(Dictionary<string, object> dataView, string templatePath)
        {
            var files = Directory.EnumerateFiles(templatePath, "*", SearchOption.AllDirectories).ToList();
            var json = File.ReadAllText(Path.Combine(templatePath, "sourceExclusions.json"));
            IDictionary<string, string>
                allExclusions = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
            var conditionalInclusions = allExclusions.Where(x => !dataView.ContainsKey(x.Key)
                                                                 || (dataView[x.Key] is bool boolValue && !boolValue)
                                                                 || (dataView[x.Key] is string stringValue &&
                                                                     stringValue != "true")).ToList();
            var exclusionFiles = new List<string>();

            foreach (string file in files)
            {
                var pathPrefix = file.Replace(Path.GetFullPath(templatePath), string.Empty)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                foreach (var inclusion in conditionalInclusions)
                {
                    var inclusionExpression = inclusion.Value;

                    if (inclusionExpression.EndsWith("**"))
                    {
                        if (pathPrefix.StartsWith(inclusionExpression.Replace("/**", string.Empty)))
                        {
                            exclusionFiles.Add(file);
                        }
                    }
                    else
                    {
                        // exact Match
                        if (pathPrefix == inclusionExpression)
                        {
                            exclusionFiles.Add(file);
                        }
                    }
                }
            }

            return files.Where(f => exclusionFiles.All(e => e != f));
        }

        public MustacheConfigSchema GetMustacheConfigSchema(string templatePath)
        {
            var json = File.ReadAllText(Path.Combine(templatePath, "mustache.json"));
            return JsonConvert.DeserializeObject<MustacheConfigSchema>(json);
        }

        public Dictionary<string, object> GetDataView(string templatePath, GeneratorModel model)
        {
            var mustacheConfig = GetMustacheConfigSchema(templatePath);
            var dataView = new Dictionary<string, object>();

            if (mustacheConfig == null)
            {
                throw new InvalidDataException($"could not find config at {templatePath}");
            }

            foreach (var dep in mustacheConfig.Params)
            {
                var defaultValue = bool.TryParse(dep.DefaultValue, out var boolDefaultValue)
                    ? (object)boolDefaultValue
                    : dep.DefaultValue;
                dataView.Add(dep.Name, defaultValue);
            }

            if (model.Dependencies != null)
            {
                foreach (var dependencyName in model.Dependencies)
                {
                    var key = mustacheConfig.Params.FirstOrDefault(k => k.Name.ToLower() == dependencyName)?.Name;
                    if (key != null)
                    {
                        dataView[key] = true;
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

            foreach (var calculatedParam in mustacheConfig.CalculatedParams)
            {
                switch (calculatedParam.ExpressionType)
                {
                    case ExpressionTypeEnum.Lookup:
                        var lambda = BuildLookupLambda(calculatedParam, dataView);
                        EvaluateLambdaExpression<bool>(calculatedParam.Name, lambda, dataView);
                        break;

                    case ExpressionTypeEnum.Lambda:
                    default:
                        lambda = calculatedParam.Expression;
                        EvaluateLambdaExpression<string>(calculatedParam.Name, lambda, dataView);

                        break;
                }
            }

            return dataView;
        }

        public string BuildLookupLambda(CalculatedParam calculatedParam, Dictionary<string, object> dataView)
        {
            var lambda = string.Empty;

            var expressionString = calculatedParam.Expression;
            var tree = SyntaxFactory.ParseExpression(expressionString);

            foreach (var node in tree.DescendantTokens())
            {
                var key = node.ToString();
                if (dataView.ContainsKey(key))
                {
                    lambda += $"dataView[\"{key}\"]";
                }
                else
                {
                    lambda += key;
                }
            }

            return "dataView => " + lambda;
        }

        public void EvaluateLambdaExpression<T>(
            string calculatedParamName,
            string lambda,
            Dictionary<string, object> dataView)
            where T : IConvertible
        {
            try
            {
                var options = ScriptOptions.Default;
                var evalExpression = CSharpScript
                    .EvaluateAsync<Func<Dictionary<string, T>, T>>(lambda, options).Result;

                var typedDataView = GetTypedDictionary<T>(dataView);

                var result = evalExpression(typedDataView);
                if (result != null)
                {
                    dataView.TryAdd(calculatedParamName, result);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(lambda, ex);
            }
        }

        private Dictionary<string, T> GetTypedDictionary<T>(Dictionary<string, object> dictionary)
            where T : IConvertible
        {
            var result = new Dictionary<string, T>();
            foreach (var (key, value) in dictionary)
            {
                if (value is T itemValue)
                {
                    result.Add(key, itemValue);
                }
            }

            return result;
        }
    }
}
