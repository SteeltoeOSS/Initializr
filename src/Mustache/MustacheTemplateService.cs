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

        public MustacheTemplateService(ILogger<MustacheTemplateService> logger )
        {

            _stubble = new StubbleBuilder()
                .Configure(settings => settings.AddJsonNet())
                .Build();
            _logger = logger;
            _templatePath = AppDomain.CurrentDomain.BaseDirectory + "templates";
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
            var name = "WebApi-CSharp-Mustache";

            var templatePath = _templatePath + Path.DirectorySeparatorChar + name;
            var dataView = GetDataView(templatePath);

            if (model.Dependencies != null)
            {
                foreach (var dependency in model.Dependencies)
                {
                    var key = dataView.Keys.FirstOrDefault(k => k.ToLower() == dependency);
                    if (key != null)
                    {
                        dataView[key] = "true";
                    }
                }
            }

            CalculatedFields(dataView, model.ProjectName);

            var listoffiles = new List<KeyValuePair<string, string>>();
           
            foreach (var file in GetFilteredSourceSets(dataView, templatePath))
            {
                if (file.EndsWith("mustache.json"))
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
                else// if (file.EndsWith(".cs"))
                {
                    var output = Render(file, fileText, dataView);
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, output));
                }

                //else
                //{
                //    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, fileText));
                //}
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
            string current = Directory.GetCurrentDirectory();
            var templatesPath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar;
            return Directory
                .GetDirectories(templatesPath)
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
            if (string.IsNullOrEmpty(shortName))
            {
                shortName = "WebApi-CSharp-Mustache";
            }

            var list = GetAvailableTemplates();
            var selectedTemplate = list.Where(x => x.ShortName == shortName).FirstOrDefault();

            var templatePath = _templatePath + Path.DirectorySeparatorChar + selectedTemplate.Name;
            var config = GetMustacheConfigJson(templatePath);

            return config
                .Where(p => p.Value != null && p.Value.ContainsKey("description") && p.Value["description"].ToString().ToLower().Contains("steeltoe"))
                .Select(p => new ProjectDependency
                {
                    Name = GetFriendlyName(p.Key),
                    ShortName = p.Key,
                    Description = p.Value.ContainsKey("description") ? p.Value["description"].ToString() : string.Empty,
                }).ToList();
        }

        private Dictionary<string, Dictionary<string, object>> GetMustacheConfigJson(string templatePath)
        {
           //var templatePath = _templatePath + Path.DirectorySeparatorChar + templateName;
            var json = File.ReadAllText(Path.Combine(templatePath, "mustache.json"));
            var dataView = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);
            return dataView;
        }

        private Dictionary<string, string> GetDataView(string templateName)
        {
            var mustacheConfig = GetMustacheConfigJson(templateName);
            var dataView = new Dictionary<string, string>();
            foreach (string key in mustacheConfig.Keys)
            {
                dataView.Add(key, mustacheConfig[key]["defaultValue"].ToString());
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