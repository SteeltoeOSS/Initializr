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
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.Utils;
using Steeltoe.Initializr.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Services
{
    public class TemplateService : ITemplateService
    {
        public Dictionary<string, string> FriendlyNames { get; set; }

        private readonly string _hivePath;
        private readonly string _outPath;

        public TemplateService(IConfiguration configuration)
            : this()
        {
            configuration.Bind(this); // Get friendlyNames
        }

        public TemplateService()
        {
            _hivePath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar;
            _outPath = AppDomain.CurrentDomain.BaseDirectory + "output" + Path.DirectorySeparatorChar;

            Console.WriteLine("hivePath " + _hivePath);
            var settingsPath = Path.Combine(_hivePath, "settings.json");
            var settingsContent = File.ReadAllText(settingsPath);

            var escapedPath = _hivePath.Replace(@"\", @"\\");
            var newContent = settingsContent.Replace("__Path__", (Path.DirectorySeparatorChar == '\\') ? escapedPath : _hivePath);

            File.WriteAllText(settingsPath, newContent);
        }

        public async Task<string> GenerateProject(string templateShortName, string projectName, string[] templateParameters)
        {
            var randomString = Guid.NewGuid().ToString() + DateTime.Now.Millisecond;
            var outFolder = Path.Combine(_outPath, randomString, projectName);

            var envSettings = GetEngineEnvironmentSettings();

            TemplateCreator creator = new TemplateCreator(envSettings);

            var iParams = new Dictionary<string, string> { { "Name", projectName } };
            foreach (var p in templateParameters)
            {
                if (p.Contains('='))
                {
                    var paramkvp = p.Split('=');
                    if (paramkvp.Length == 2)
                    {
                        iParams.Add(paramkvp[0], paramkvp[1]);
                    }
                }
                else
                {
                    iParams.Add(p, "true");
                }
            }

            TemplateInfo templateInfo = FindTemplateByShortName(templateShortName, envSettings);
            if (templateInfo == null)
            {
                throw new Exception($"Could not find template with shortName: {templateShortName} ");
            }

            await creator.InstantiateAsync(templateInfo, projectName, "SteeltoeProject", outFolder, iParams, true, false, "baseLine");

            return outFolder;
        }

        public List<TemplateViewModel> GetAvailableTemplates()
        {
            var list = GetAllTemplates();
            var items = list.Select(x => new TemplateViewModel
            {
                Name = x.Name,
                ShortName = x.ShortName,
                Language = x.Parameters?.FirstOrDefault(p => p.Name == "language")?.DefaultValue,
                Tags = x.Classifications.Aggregate((current, next) => current + "/" + next),
            });
            return items.ToList();
        }

        public List<ProjectDependency> GetDependencies(string shortName = "steeltoe2")
        {
            var list = GetAllTemplates();
            var selectedTemplate = list.Where(x => x.ShortName == shortName).FirstOrDefault();
            return selectedTemplate.Parameters
                .Where(p => p.Documentation != null && p.Documentation.ToLower().Contains("steeltoe"))
                .Select(p => new ProjectDependency
                {
                    Name = GetFriendlyName(p.Name),
                    ShortName = p.Name,
                    Description = p.Documentation,
                }).ToList();
        }

        private EngineEnvironmentSettings GetEngineEnvironmentSettings()
        {
            var envSettings = new EngineEnvironmentSettings(
                 new DefaultTemplateEngineHost("Initializr", "v1.0", string.Empty),
                 (IEngineEnvironmentSettings settings) => new InitializrSettingsLoader(settings, _hivePath),
                 _hivePath);
            var cachePath = Path.Combine(_hivePath, "templatecache.json");

            if (!File.Exists(cachePath))
            {
                Console.WriteLine("File doesnt exist " + cachePath);
                ((InitializrSettingsLoader)envSettings.SettingsLoader).RebuildCacheFromSettingsIfNotCurrent(true);
            }

            return envSettings;
        }

        private TemplateInfo FindTemplateByShortName(string shortName, IEngineEnvironmentSettings envSettings)
        {
            var loader = (InitializrSettingsLoader)envSettings.SettingsLoader;
            return loader.UserTemplateCache.TemplateInfo.Where(ti => (bool)ti.ShortNameList?.Contains(shortName)).FirstOrDefault();
        }

        private IReadOnlyList<TemplateInfo> GetAllTemplates()
        {
            var envSettings = GetEngineEnvironmentSettings();

            return ((InitializrSettingsLoader)envSettings.SettingsLoader).UserTemplateCache.TemplateInfo;
        }

        private string GetFriendlyName(string name)
        {
            return FriendlyNames?.ContainsKey(name) == true ? FriendlyNames[name] : name;
        }
    }
}