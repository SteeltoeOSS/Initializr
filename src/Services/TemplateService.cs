using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InitializrApi.Models;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Cli.PostActionProcessors;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.Edge.TemplateUpdates;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Utils;

namespace InitializrApi.Services
{
    public class TemplateService : ITemplateService
    {
   
        private string _hivePath;
        private string _outPath;
        public TemplateService()
        {
            _hivePath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar;
            _outPath = AppDomain.CurrentDomain.BaseDirectory + "output" + Path.DirectorySeparatorChar;

            Console.WriteLine("hivePath " + _hivePath);
            var settingsPath = Path.Combine(_hivePath, "settings.json");
            var settingsContent = File.ReadAllText(settingsPath);

            var escapedPath = _hivePath.Replace(@"\", @"\\");
            var newContent = settingsContent.Replace("__Path__", (Path.DirectorySeparatorChar == '\\')? escapedPath: _hivePath);
            File.WriteAllText(settingsPath, newContent);

            // _env = env;
        }
        public async Task<string> GenerateProject(string templateShortName, string ProjectName, string[] TemplateParameters)
        {
            var randomString = Guid.NewGuid().ToString() + DateTime.Now.Millisecond;
            var outFolder = Path.Combine(_outPath, randomString);
     
            var envSettings = new EngineEnvironmentSettings(
                 new DefaultTemplateEngineHost("Initializr", "v1.0", ""),
                 (IEngineEnvironmentSettings settings) => new InitializrSettingsLoader(settings, _hivePath),
                 _hivePath);
            var cachePath = Path.Combine(_hivePath, "templatecache.json");

            if (!File.Exists(cachePath))
            {
                Console.WriteLine("File doesnt exist " + cachePath);
                ((InitializrSettingsLoader)envSettings.SettingsLoader).RebuildCacheFromSettingsIfNotCurrent(true);
            }

            TemplateCreator creator = new TemplateCreator(envSettings);

            var iParams = new Dictionary<string, string> { { "Name", ProjectName } };
            foreach (var p in TemplateParameters)
            {
                if (p.Contains('='))
                {
                    var paramkvp = p.Split('=');
                    if(paramkvp.Length == 2 )
                    {
                        iParams.Add(paramkvp[0], paramkvp[1]);
                    }
                }
                else
                {
                    iParams.Add(p, "true");
                }
            }

            TemplateInfo templateInfo = FindTemplateByShortName(templateShortName,  envSettings);
            if(templateInfo == null)
            {
                throw new Exception($"Could not find template with shortName: {templateShortName} ");
            }
            await creator.InstantiateAsync(templateInfo, ProjectName, "SteeltoeProject", outFolder, iParams, true, false, "baseLine");

            return outFolder;
            
        }

        private TemplateInfo FindTemplateByShortName(string shortName, IEngineEnvironmentSettings envSettings)
        {
            return ((InitializrSettingsLoader)envSettings.SettingsLoader).UserTemplateCache.TemplateInfo.Where(ti => (bool)(ti.ShortNameList?.Contains(shortName))).FirstOrDefault();
            
        }

        private IReadOnlyCollection<ITemplateMatchInfo> GetAllTemplates()
        {
            var host = CreateHost(true);
            var EnvironmentSettings = new EngineEnvironmentSettings(host, x => new SettingsLoader(x), _hivePath);
            var _settingsLoader = (SettingsLoader)EnvironmentSettings.SettingsLoader;
            var _hostDataLoader = new HostSpecificDataLoader(EnvironmentSettings.SettingsLoader);
                var list = TemplateListResolver.PerformAllTemplatesQuery(_settingsLoader.UserTemplateCache.TemplateInfo, _hostDataLoader);
            return list;
        }

        private static DefaultTemplateEngineHost CreateHost(bool emitTimings)
        {
            var preferences = new Dictionary<string, string>
            {
                { "prefs:language", "C#" }
            };

            try
            {
                string versionString = Dotnet.Version().CaptureStdOut().Execute().StdOut;
                if (!string.IsNullOrWhiteSpace(versionString))
                {
                    preferences["dotnet-cli-version"] = versionString.Trim();
                }
            }
            catch
            { }

            var builtIns = new AssemblyComponentCatalog(new[]
            {
                typeof(RunnableProjectGenerator).GetTypeInfo().Assembly,            // for assembly: Microsoft.TemplateEngine.Orchestrator.RunnableProjects
                typeof(NupkgInstallUnitDescriptorFactory).GetTypeInfo().Assembly,   // for assembly: Microsoft.TemplateEngine.Edge
                typeof(DotnetRestorePostActionProcessor).GetTypeInfo().Assembly     // for assembly: Microsoft.TemplateEngine.Cli
            });

            DefaultTemplateEngineHost host = new DefaultTemplateEngineHost("Initializr", "1.0", CultureInfo.CurrentCulture.Name, preferences, builtIns, new[] { "dotnetcli" });

            if (emitTimings)
            {
                host.OnLogTiming = (label, duration, depth) =>
                {
                    string indent = string.Join("", Enumerable.Repeat("  ", depth));
                    Console.WriteLine($"{indent} {label} {duration.TotalMilliseconds}");
                };
            }


            return host;
        }
        public List<TemplateViewModel> GetAvailableTemplates()
        {
            var list = GetAllTemplates();
            var items = list.Select(x => new TemplateViewModel
            {
                Id = x.Info.Identity,
                Name = x.Info.Name,
                ShortName = x.Info.ShortName,
                Language = x.Info.Parameters?.FirstOrDefault(p => p.Name == "language")?.DefaultValue,
                Tags = x.Info.Classifications.Aggregate((current, next) => current + "/" + next)
            });
            return items.ToList();
        }

        public List<ProjectDependency> GetDependencies(string shortName = "steeltoe")
        {

            var list = GetAllTemplates();
            var selectedTemplate =  list.Where(x => x.Info.ShortName == shortName).FirstOrDefault();
            return selectedTemplate.Info.Parameters
                .Where(p=> p.Documentation != null && p.Documentation.ToLower().Contains("steeltoe"))
                .Select(p => new ProjectDependency
                {
                    Name = p.Name,
                    Description = p.Documentation
                }).ToList();
            
        }
    }
}
