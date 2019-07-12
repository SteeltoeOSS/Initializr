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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steeltoe.Initializr.Models;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Extensions.JsonNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Services
{
    public class MustacheTemplateService : ITemplateService
    {
        private const string DEFAULT_TEMPLATE_NAME = "CSharp-WebApi-2.x";

        public Dictionary<string, string> FriendlyNames { get; set; }

        private StubbleVisitorRenderer _stubble;
        private ILogger<MustacheTemplateService> _logger;
        private string _templatePath;

        private class Account
        {
            public string Email { get; set; }

            public Func<string, object> MyLambda { get; set; }
        }

        public MustacheTemplateService(ILogger<MustacheTemplateService> logger, IConfiguration configuration)
            : this(logger)
        {

            configuration.Bind(this); // Get friendlyNames
        }

        public MustacheTemplateService(ILogger<MustacheTemplateService> logger)
        {
            _stubble = new StubbleBuilder()
                .Configure(settings => settings.AddJsonNet())
                .Build();
            _logger = logger;
            _templatePath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar + "Mustache";
        }

        public async Task<byte[]> GenerateProjectArchive(GeneratorModel model)
        {
            byte[] archiveBytes;
            List<KeyValuePair<string, string>> listoffiles = GenerateProjectFiles(model);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var entry in listoffiles)
                    {
                        _logger.LogDebug(entry.Key);
                        var ef = archive.CreateEntry(entry.Key, CompressionLevel.Optimal);
                        using (var entryStream = ef.Open())
                        using (var fileToCompress = new MemoryStream(Encoding.UTF8.GetBytes(entry.Value)))
                        {
                            fileToCompress.CopyTo(entryStream);
                        }
                    }
                }

                archiveBytes = memoryStream.ToArray();
            }

            return archiveBytes;
        }

        public List<KeyValuePair<string, string>> GenerateProjectFiles(GeneratorModel model)
        {
            var name = string.IsNullOrEmpty(model.TemplateShortName) ? DEFAULT_TEMPLATE_NAME : model.TemplateShortName;

            var templatePath = _templatePath + Path.DirectorySeparatorChar + name;

            if (!Directory.Exists(templatePath))
            {
                throw new InvalidDataException("Template with $name doesnt exist");
            }

            var dataView = GetDataView(templatePath, model);

            CalculatedFields(dataView, model.ProjectName);

            var listoffiles = new List<KeyValuePair<string, string>>();

            foreach (var file in GetFilteredSourceSets(dataView, templatePath))
            {
                if (file.EndsWith("mustache.json")
                    || file.EndsWith("sourceExclusions.json"))
                {
                    continue;
                }

                var pathPrefix = file.Replace(Path.GetFullPath(templatePath), string.Empty).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                string fileText = File.ReadAllText(file);

                if (file.EndsWith(".csproj"))
                {
                    pathPrefix = pathPrefix.Replace("ReplaceMe", model.ProjectName ?? "SteeltoeExample"); // get from model
                    var output = Render(file, fileText, dataView);
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, output));
                }
                else
                {
                    var output = Render(file, fileText, dataView);
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, output));
                }
            }

            return listoffiles;
        }

        private IEnumerable<string> GetFilteredSourceSets(Dictionary<string, string> dataView, string templatePath)
        {
            var files = Directory.EnumerateFiles(templatePath, "*", SearchOption.AllDirectories);
            var json = File.ReadAllText(Path.Combine(templatePath, "sourceExclusions.json"));
            IDictionary<string, string> allExclusions = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
            var applicableExclusions = allExclusions.Where(x => !dataView.ContainsKey(x.Key) || dataView[x.Key] != "true");
            var exclusionFiles = new List<string>();

            foreach (string file in files)
            {
                var pathPrefix = file.Replace(Path.GetFullPath(templatePath), string.Empty).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                foreach (var exclusion in applicableExclusions)
                {
                    var exclusionExpression = exclusion.Value;

                    if (exclusionExpression.EndsWith("**"))
                    {
                        if (pathPrefix.StartsWith(exclusionExpression.Replace("/**", string.Empty)))
                        {
                            exclusionFiles.Add(file);
                        }
                    }
                    else // exact Match
                    {
                        if (pathPrefix == exclusionExpression)
                        {
                            exclusionFiles.Add(file);
                        }
                    }
                }
            }
            return files.Where(f => !exclusionFiles.Any(e => e == f));
        }

        private void CalculatedFields(Dictionary<string, string> dataView, string projectName)
        {
            // Add Calculated deps
            if (dataView["Actuators"] == "true" || dataView["CloudFoundry"] == "true")
            {
                dataView.Add("ActuatorsOrCloudFoundry", "true");
            }

            if (dataView["MySql"] == "true" || dataView["MySqlEFCore"] == "true")
            {
                dataView.Add("MySqlOrMySqlEFCore", "true");
            }

            if (dataView["Actuators"] == "true" || dataView["DynamicLogger"] == "true")
            {
                dataView.Add("ActuatorsOrDynamicLogger", "true");
            }

            if (dataView["SQLServer"] == "true" || dataView["MySqlEFCore"] == "true")
            {
                dataView.Add("AnyEFCore", "true");
            }

            if (dataView["MySql"] == "true"
                || dataView["Postgres"] == "true"
                || dataView["Redis"] == "true"
                || dataView["MongoDB"] == "true"
                || dataView["OAuthConnector"] == "true")
            {
                dataView.Add("AnyConnector", "true");
            }

            if (dataView["TargetFrameworkVersion"] == "netcoreapp2.2")
            {
                dataView.Add("AspNetCoreVersion", "2.2.0");
                dataView.Add("TargetFrameworkVersion22", "true");
            }
            else if (dataView["TargetFrameworkVersion"] == "netcoreapp2.1")
            {
                dataView.Add("AspNetCoreVersion", "2.1.1");
            }

            dataView.Add("ProjectNameSpace", projectName);
        }

        private string Render(string name, string input, object view)
        {
            try
            {
                return _stubble.Render(input, view);
            }
            catch (Exception ex)
            {
                throw new Exception("Error rendering " + name, ex);
            }
        }

        public List<TemplateViewModel> GetAvailableTemplates()
        {
            return Directory
                .GetDirectories(_templatePath)
                .Select(path => new TemplateViewModel
                {
                    Name = new DirectoryInfo(path).Name,
                    ShortName = new DirectoryInfo(path).Name,
                    Language = "C#",
                    Tags = "Web/Microservice",
                })
                .ToList();

        }

        public List<ProjectDependency> GetDependencies(string shortName)
        {
            shortName = string.IsNullOrEmpty(shortName) ? DEFAULT_TEMPLATE_NAME : shortName;
            var list = GetAvailableTemplates();
            var selectedTemplate = list.Where(x => x.ShortName == shortName).FirstOrDefault();

            if (selectedTemplate == null)
            {
                throw new InvalidDataException($"Could not find template with name {shortName} ");
            }

            var templatePath = _templatePath + Path.DirectorySeparatorChar + selectedTemplate.Name;
            var config = GetMustacheConfigJson(templatePath);

            return config.Dependencies
                .Where(p => p.Description.ToLower().Contains("steeltoe"))
                .Select(p => new ProjectDependency
                {
                    Name = GetFriendlyName(p.Name),
                    ShortName = p.Name,
                    Description = p.Description,
                }).ToList();
        }

        private MustacheConfig GetMustacheConfigJson(string templatePath)
        {
            var json = File.ReadAllText(Path.Combine(templatePath, "mustache.json"));
            return JsonConvert.DeserializeObject<MustacheConfig>(json);
        }

        private Dictionary<string, string> GetDataView(string templatePath, GeneratorModel model)
        {
            var mustacheConfig = GetMustacheConfigJson(templatePath);
            var dataView = new Dictionary<string, string>();

            if (mustacheConfig == null)
            {
                throw new InvalidDataException($"could not find config at {templatePath}");
            }

            foreach (var dep in mustacheConfig.Dependencies)
            {
                dataView.Add(dep.Name, dep.DefaultValue);
            }

            if (model.Dependencies != null)
            {
                foreach (var dependencyName in model.Dependencies)
                {
                    var key = mustacheConfig.Dependencies.FirstOrDefault(k => k.Name.ToLower() == dependencyName)?.Name;
                    if (key != null)
                    {
                        dataView[key] = "true";
                    }
                }
            }

            foreach (var version in mustacheConfig.Versions)
            {
                dataView.Add(version.Name, version.DefaultValue);
            }

            if (model.SteeltoeVersion != null)
            {
                var steeltoeVersionName = "SteeltoeVersion";
                var steeltoVersion = mustacheConfig.Versions.FirstOrDefault(v => v.Name == steeltoeVersionName);
                if (steeltoVersion.Choices.Any(choice => model.SteeltoeVersion.ToLower() == choice.Choice))
                {
                    dataView[steeltoeVersionName] = model.SteeltoeVersion.ToLower();
                }
            }

            if (model.TargetFrameworkVersion != null)
            {
                var targetFmwkName = "TargetFrameworkVersion";
                var targetFmwk = mustacheConfig.Versions.FirstOrDefault(v => v.Name == targetFmwkName);

                if (targetFmwk.Choices.Any(choice => model.TargetFrameworkVersion.ToLower() == choice.Choice))
                {
                    dataView[targetFmwkName] = model.TargetFrameworkVersion;
                }
            }

            return dataView;
        }

        private string GetFriendlyName(string name)
        {
            return FriendlyNames?.ContainsKey(name) == true ? FriendlyNames[name] : name;
        }

        public void ClearCache()
        {
            throw new NotImplementedException();
        }
    }
}