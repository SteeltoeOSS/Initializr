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

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.Utils;
using Steeltoe.Initializr.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Services.DotNetTemplateEngine
{
    public class TemplateService : ITemplateService
    {
        public Dictionary<string, string> FriendlyNames { get; set; }

        private static readonly string ENV_SETTINGS_KEY = "Environment_Settings";
        private static readonly string TEMPLATE_CACHE_KEY = "Template_Cache";
        private static readonly string DEFAULT_TEMPLATE = "CSharp-WebApi-2.x";

        private EngineEnvironmentSettings EnvSettings
        {
            get
            {
                EngineEnvironmentSettings settings;

                if (_memoryCache == null || !_memoryCache.TryGetValue(ENV_SETTINGS_KEY, out settings))
                {
                    settings = GetEngineEnvironmentSettings();
                    _memoryCache?.Set(ENV_SETTINGS_KEY, settings, new TimeSpan(24, 0, 0));
                }

                return settings;
            }
        }

        private InitializrTemplateCache TemplateCache
        {
            get
            {
                InitializrTemplateCache templateCache;

                if (_memoryCache == null || !_memoryCache.TryGetValue(ENV_SETTINGS_KEY, out templateCache))
                {
                    templateCache = ((InitializrSettingsLoader)EnvSettings.SettingsLoader).UserTemplateCache;
                    _memoryCache?.Set(TEMPLATE_CACHE_KEY, templateCache, new TimeSpan(24, 0, 0));
                }

                return templateCache;
            }
        }

        private readonly string _hivePath;
        private readonly string _outPath;
        private IMemoryCache _memoryCache;
        private ILogger<TemplateService> _logger;

        public TemplateService(IConfiguration configuration, IMemoryCache memoryCache, ILogger<TemplateService> logger)
            : this(logger)
        {
            _memoryCache = memoryCache;
            configuration.Bind(this); // Get friendlyNames
        }

        public TemplateService(ILogger<TemplateService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _hivePath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar + "DotNetTemplating" + Path.DirectorySeparatorChar;
            _outPath = AppDomain.CurrentDomain.BaseDirectory + "output" + Path.DirectorySeparatorChar;

            Console.WriteLine("hivePath " + _hivePath);
            var settingsPath = Path.Combine(_hivePath, "settings.json");
            var settingsContent = File.ReadAllText(settingsPath);

            var escapedPath = _hivePath.Replace(@"\", @"\\");
            var newContent = settingsContent.Replace("__Path__", (Path.DirectorySeparatorChar == '\\') ? escapedPath : _hivePath);

            File.WriteAllText(settingsPath, newContent);
        }

        public void ClearCache()
        {
            _memoryCache?.Remove(ENV_SETTINGS_KEY);
        }

        public async Task<string> GenerateProject(GeneratorModel model)
        {
            var randomString = Guid.NewGuid().ToString() + DateTime.Now.Millisecond;
            var outFolder = Path.Combine(_outPath, randomString, model.ProjectName);

            var iParams = new Dictionary<string, string> { { "Name", model.ProjectName } };
            foreach (var p in model.GetTemplateParameters())
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

            if (!string.IsNullOrEmpty(model.TargetFrameworkVersion))
            {
                iParams.Add("Framework", model.TargetFrameworkVersion);
            }

            var templateShortName = string.IsNullOrEmpty(model.TemplateShortName) ? DEFAULT_TEMPLATE : model.TemplateShortName;

            TemplateInfo templateInfo = FindTemplateByShortName(templateShortName, EnvSettings);
            if (templateInfo == null)
            {
                throw new Exception($"Could not find template with shortName: {templateShortName} ");
            }

            TemplateCreator creator = new TemplateCreator(EnvSettings);
            var creationResult = await creator.InstantiateAsync(templateInfo, model.ProjectName, "SteeltoeProject",
                outFolder, iParams, true, false, "baseLine");

            if (creationResult.Status != CreationResultStatus.Success)
            {
                throw new InvalidDataException(creationResult.Message + ": " + creationResult.Status + " " + templateShortName);
            }

            return outFolder;
        }

        public async Task<List<KeyValuePair<string, string>>> GenerateProjectFiles(GeneratorModel model)
        {
            var listOfFiles = new List<KeyValuePair<string, string>>();

            var outFolder = await GenerateProject(model);
            foreach (var file in Directory.EnumerateFiles(outFolder, "*", SearchOption.AllDirectories))
            {
                var pathPrefix = file.Replace(Path.GetFullPath(outFolder), string.Empty).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string fileText = File.ReadAllText(file);
                listOfFiles.Add(new KeyValuePair<string, string>(pathPrefix, fileText));
            }

            return listOfFiles;
        }

        public async Task<byte[]> GenerateProjectArchiveAsync(GeneratorModel model)
        {
            var outFolder = await GenerateProject(model);

            var zipFile = Path.Combine(outFolder, "..", model.ArchiveName);

            await Task.Run(() => ZipFile.CreateFromDirectory(outFolder, zipFile));
            var archiveBytes = await System.IO.File.ReadAllBytesAsync(zipFile);
            await Task.Run(() => Delete(outFolder, zipFile));
            return archiveBytes;
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

        public List<ProjectDependency> GetDependencies(string shortName)
        {
            var list = GetAllTemplates();

            shortName = string.IsNullOrEmpty(shortName) ? DEFAULT_TEMPLATE : shortName;

            var selectedTemplate = list.FirstOrDefault(x => x.ShortName == shortName);

            if (selectedTemplate == null)
            {
                throw new InvalidDataException($"Could not find template with ShortName: {shortName ?? DEFAULT_TEMPLATE} ");
            }

            return selectedTemplate.Parameters
                .Where(p => p.Documentation != null && p.Documentation.ToLower().StartsWith("steeltoe:"))
                .Select(p => new ProjectDependency
                {
                    Name = GetFriendlyName(p.Name),
                    ShortName = p.Name,
                    Description = p.Documentation,
                }).ToList();
        }

        private void Delete(string outFolder, string zipFile)
        {
            try
            {
                System.IO.File.Delete(zipFile);
                Directory.Delete(Path.Combine(outFolder, ".."), true);
            }
            catch (Exception ex)
            {
               _logger.LogError("Delete Error: " + ex.Message);
            }
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
            return TemplateCache.TemplateInfo;
        }

        private string GetFriendlyName(string name)
        {
            return FriendlyNames?.ContainsKey(name) == true ? FriendlyNames[name] : name;
        }
    }
}