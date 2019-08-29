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
using Steeltoe.Initializr.Services;
using Steeltoe.Initializr.Services.Mustache;
using Steeltoe.InitializrTests;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.Initializr.Tests
{
    [Trait("Category", "Integration")]
    public class IntegrationTests : XunitLoggingBase
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly LoggerFactory _loggerFactory;

        public IntegrationTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_Test(ITemplateService templateService, string templateName, TemplateVersion version)
        {
            var deps = templateService.GetDependencies(templateName, version);

            foreach (var dep in deps)
            {
                _testOutputHelper.WriteLine($"testing  dep: --" + dep.ShortName);

                // if (templateService is MustacheTemplateService) return;
                // if (!templateName.Contains("React")) return;
                // if (version == TemplateVersion.V3) return;
                var archive = await templateService.GenerateProjectArchiveAsync(new Models.GeneratorModel()
                {
                    // Todo : fix this
                    Dependencies = dep.ShortName,
                    TemplateShortName = templateName,
                    ProjectName = "Foo.Bar",
                    TemplateVersion = version,
                });
                var zip = new ZipArchive(new MemoryStream(archive));
                var dirName = Path.GetTempPath() + Path.DirectorySeparatorChar + Guid.NewGuid();
                zip.ExtractToDirectory(dirName);

                var startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = "dotnet";
                startInfo.Arguments = "build";
                startInfo.WorkingDirectory = dirName;
                var process = Process.Start(startInfo);

                while (!process.HasExited)
                {
                    Thread.Sleep(100);
                }

                var output = process.StandardOutput.ReadToEnd();
                Assert.Contains("Build succeeded.", output);
            }
        }
    }
}