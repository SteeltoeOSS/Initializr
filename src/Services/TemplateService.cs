using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InitializrApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Cli.PostActionProcessors;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.Edge.TemplateUpdates;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Utils;
using Newtonsoft.Json.Linq;

namespace InitializrApi.Services
{
    public class TemplateService : ITemplateService
    {
   
        private string hivePath;
        //private string outFolder = "";

        //    IHostingEnvironment _env;
        public TemplateService()
        {
            hivePath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar;
            Console.WriteLine("hivePath " + hivePath);
            var settingsPath = Path.Combine(hivePath, "settings.json");
            var settingsContent = File.ReadAllText(settingsPath);

            var escapedPath = hivePath.Replace(@"\", @"\\");
            var newContent = settingsContent.Replace("__Path__", (Path.DirectorySeparatorChar == '\\')? escapedPath: hivePath);
            File.WriteAllText(settingsPath, newContent);

            // _env = env;
        }
        public async Task<string> GenerateProject()
        {

            var output = Path.Combine("\\", "output", Guid.NewGuid().ToString() + DateTime.Now.Millisecond);
            var outFolder = Path.Combine(output, "generated");

            ITemplateEngineHost host = new DefaultTemplateEngineHost("Initializr", "v1.0", "");

            var cachePath = Path.Combine(hivePath, "templateCache.json");

            Func<IEngineEnvironmentSettings, ISettingsLoader> settingsLoaderFactory = (IEngineEnvironmentSettings settings) =>
            {
                var loader = new InitializrSettingsLoader(settings, hivePath);
                if (!System.IO.File.Exists(cachePath))
                {
                    loader.RebuildCacheFromSettingsIfNotCurrent(true);
                }
                return loader;
            };

            var envSettings = new EngineEnvironmentSettings(host, settingsLoaderFactory, hivePath);
          

            TemplateCreator creator = new TemplateCreator(envSettings);



            string usrTemplateCache = System.IO.File.ReadAllText(cachePath);
            JObject parsedCache = JObject.Parse(usrTemplateCache);
            var tc = new TemplateCache(envSettings, parsedCache, "");
            var myTemplateInfo = tc.TemplateInfo[0];

            //ITemplateInfo templateInfo = TemplateInfo.FromJObject()
            //{

            //}
            var iParams = new Dictionary<string, string>
            {
                { "Name", "SteProject" },
                {"Actuators", "false"}
            };

            TemplateCreationResult instantiateResult;
            try
            {
                instantiateResult = await creator.InstantiateAsync(myTemplateInfo, "myName", "noIdea", outFolder, iParams, true, false, "baseLine");
            }
            catch (Exception ex)
            {
                instantiateResult = new TemplateCreationResult(ex.Message + ex.StackTrace, CreationResultStatus.CreateFailed, outFolder);
            }
            return instantiateResult.Message + " " + instantiateResult.OutputBaseDirectory;
        }
    
        //public string GenerateProject(GeneratorModel model)
        //{
        //    var host = CreateHost(false);

        //    StringWriter stringWriter = new StringWriter();
        //  //  Console.SetOut(stringWriter);
        //    //var str = "";
        //    //using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        //    //{
        //    //    str =  reader.ReadToEndAsync().Result;
        //    //}
        //    var output = Path.Combine(@"\", "output");
        //    var outFolder = Path.Combine(output, "generated");
        //    var zipfile = Path.Combine(output, $"{model.projectName}project.zip");

        //    if (Directory.Exists(outFolder))
        //    {
        //        Directory.Delete(outFolder, true);
        //    }
        //    if (File.Exists(zipfile))
        //    {
        //        File.Delete(zipfile);
        //    }


        //    var argsList = new List<string>();
        //    //  argsList.Add("-l");

        //    argsList.Add(model.projectType); // Type of project mvc, console etc
        //    if (!string.IsNullOrEmpty(model.projectName))
        //    {
        //        argsList.Add("-n");
        //        argsList.Add($"{model.projectName}");
        //    }
        //    //Todo: Add language
        //    //finally
        //    argsList.AddRange(new[] { "--output", outFolder });

        //    New3Command.Run(CommandName, host, new TelemetryLogger(null, false), FirstRun, argsList.ToArray(),hivePath);

        //    var logs = stringWriter.ToString();
        //    stringWriter.Flush();
        //    ZipFile.CreateFromDirectory(outFolder, zipfile);
        //    return zipfile;

        //}
        private IReadOnlyCollection<ITemplateMatchInfo> GetAllTemplates()
        {
            var host = CreateHost(true);
            var EnvironmentSettings = new EngineEnvironmentSettings(host, x => new SettingsLoader(x), hivePath);
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

        //private static void FirstRun(IEngineEnvironmentSettings environmentSettings, IInstaller installer)
        //{

        //    List<string> toInstallList = new List<string>();
        //    Paths paths = new Paths(environmentSettings);

        //    if (paths.FileExists(paths.Global.DefaultInstallPackageList))
        //    {
        //        toInstallList.AddRange(paths.ReadAllText(paths.Global.DefaultInstallPackageList).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        //    }

        //    if (paths.FileExists(paths.Global.DefaultInstallTemplateList))
        //    {
        //        toInstallList.AddRange(paths.ReadAllText(paths.Global.DefaultInstallTemplateList).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        //    }

        //    if (toInstallList.Count > 0)
        //    {
        //        for (int i = 0; i < toInstallList.Count; i++)
        //        {
        //            toInstallList[i] = toInstallList[i].Replace("\r", "")
        //                .Replace('\\', Path.DirectorySeparatorChar);
        //        }

        //        installer.InstallPackages(toInstallList);
        //    }
        //}

        //public List<string> GetPaths()
        //{
        //    var host = CreateHost(true);

        //    string hivePath = AppDomain.CurrentDomain.BaseDirectory;
        //    var EnvironmentSettings = new EngineEnvironmentSettings(host, x => new SettingsLoader(x), hivePath);
        //    var _settingsLoader = (SettingsLoader)EnvironmentSettings.SettingsLoader;
        //    var _hostDataLoader = new HostSpecificDataLoader(EnvironmentSettings.SettingsLoader);
        //    var list = new List<string>();
        //    list.Add(_settingsLoader.EnvironmentSettings.Paths.BaseDir);
        //    list.Add(_settingsLoader.EnvironmentSettings.Paths.UserProfileDir);

        //    var output = Path.Combine("/", "output");
        //    var outFolder = Path.Combine(output, "generated");
        //    list.Add(outFolder);
        //    return list; 
        //}

        public List<TemplateViewModel> GetAvailableTemplates()
        {
            var list = GetAllTemplates();
            var items = list.Select(x => new TemplateViewModel
            {
                Name = x.Info.Name,
                ShortName = x.Info.ShortName,
                Language = x.Info.Parameters?.FirstOrDefault(p => p.Name == "language")?.DefaultValue,
                Tags = x.Info.Classifications.Aggregate((current, next) => current + "/" + next)
            });
            return items.ToList();
        }

        //public string DebugReinstall()
        //{
        //    var host = CreateHost(false);
        //    var response = "";
        //    using (StringWriter stringWriter = new StringWriter())
        //    {
        //        Console.SetOut(stringWriter);

        //        New3Command.Run(CommandName, host, new TelemetryLogger(null, true), FirstRun, new[] { "--debug:reinit" });
        //      //  stringWriter.read
        //    }
        //    return "";
        //}
          

    }
}
