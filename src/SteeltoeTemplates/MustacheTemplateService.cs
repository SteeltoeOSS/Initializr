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

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

namespace Steeltoe.Initializr.Services
{
    public class MustacheTemplateService : IMustacheTemplateService
    {
        private StubbleVisitorRenderer _stubble;
        private ILogger<MustacheTemplateService> _logger;

        private class Account
        {
            public string Email { get; set; }

            public Func<string, object> MyLambda { get; set; }
        }

        public MustacheTemplateService(ILogger<MustacheTemplateService> logger)
        {
            _stubble = new StubbleBuilder()
                .Configure(settings => settings.AddJsonNet())
                .Build();
            _logger = logger;
        }

        public byte[] GenerateProjectZip(GeneratorModel model)
        {
            byte[] archiveBytes;
            List<KeyValuePair<string, string>> listoffiles = GenerateProject(model);

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

        public List<KeyValuePair<string, string>> GenerateProject(GeneratorModel model)
        {
            var name = "WebApi-CSharp-Mustache";
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar + name;

            var json = File.ReadAllText(Path.Combine(templatePath, "mustache.json"));
            var dataView = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
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

            CalculatedFields(dataView);

            var listoffiles = new List<KeyValuePair<string, string>>();

            foreach (var file in Directory.EnumerateFiles(templatePath, "*", SearchOption.AllDirectories))
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
                else if (file.EndsWith(".cs"))
                {
                    var output = Render(file, fileText, dataView);
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, output));
                }
                else
                {
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, fileText));
                }
            }

            return listoffiles;
        }

        private static void CalculatedFields(Dictionary<string, string> dataView)
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

        public List<string> GetAvailableTemplates()
        {
            string current = Directory.GetCurrentDirectory();
            var templatesPath = AppDomain.CurrentDomain.BaseDirectory + "templates" + Path.DirectorySeparatorChar;
            return Directory.GetDirectories(templatesPath).Select(path => new DirectoryInfo(path).Name).ToList();
        }
    }
}