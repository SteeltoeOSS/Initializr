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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.TemplateEngine.Models;
using Steeltoe.Initializr.TemplateEngine.Services;
using Steeltoe.Initializr.TemplateEngine.Services.Mustache;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.Initializr.TemplateEngine.Test.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class IntegrationTests : XunitLoggingBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public IntegrationTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        // [Fact]
        // public void Scratch()
        // {
            // var argsList = GetArgs(typeof(MustacheTemplateService), Constants.Steeltoe30, Constants.NetCoreApp31, "webapi");
            // foreach (var args in argsList)
            // {
                // _testOutputHelper.WriteLine($"steeltoe {args[1]}, dotnet {args[2]}, template {args[3]}, dependency {args[4]}");
            // }
        // }

        [Theory]
        [MemberData(nameof(GetArgs), typeof(MustacheTemplateService), Constants.Steeltoe24, Constants.NetCoreApp21, "webapi")]

        public async Task Steeltoe24Dotnet21(ITemplateService service, string steeltoe, string framework, string template, string dependency)
        {
            await GenerateAndBuildProject(service, steeltoe, framework, template, dependency);
        }

        [Theory]
        [MemberData(nameof(GetArgs), typeof(MustacheTemplateService), Constants.Steeltoe24, Constants.NetCoreApp31, "webapi")]
        public async Task Steeltoe24Dotnet31(ITemplateService service, string steeltoe, string framework, string template, string dependency)
        {
            await GenerateAndBuildProject(service, steeltoe, framework, template, dependency);
        }

        [Theory]
        [MemberData(nameof(GetArgs), typeof(MustacheTemplateService), Constants.Steeltoe30, Constants.NetCoreApp31, "webapi")]
        public async Task Steeltoe30Dotnet31(ITemplateService service, string steeltoe, string framework, string template, string dependency)
        {
            await GenerateAndBuildProject(service, steeltoe, framework, template, dependency);
        }

        // TODO: test Steeltoe 3

        // TODO: test uber projects

        private async Task GenerateAndBuildProject(
            ITemplateService service,
            string steeltoe,
            string framework,
            string template,
            string dependency)
        {
            _testOutputHelper.WriteLine(
                $"Generating and Building: Steeltoe {steeltoe}, Framework {framework}, Template {template}, dependency {dependency}");
            var archive = await service.GenerateProjectArchiveAsync(new GeneratorModel()
            {
                Dependencies = dependency,
                Template = template,
                ProjectName = "Foo.Bar",
                TargetFramework = framework,
                SteeltoeVersion = steeltoe,
            });

            var zip = new ZipArchive(new MemoryStream(archive));
            var dirName = Path.GetTempPath() + Path.DirectorySeparatorChar + Guid.NewGuid();
            _testOutputHelper.WriteLine($"Project directory: {dirName}");
            zip.ExtractToDirectory(dirName);
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = "dotnet";
            startInfo.Arguments = "build";
            startInfo.WorkingDirectory = dirName;
            var process = Process.Start(startInfo);

            await process.WaitForExitAsync();
            var output = process.StandardOutput.ReadToEnd();
            Assert.True(
                output.Contains("Build succeeded."),
                $"Error compiling {dependency}. \n {output}");
        }

        public static IEnumerable<object[]> GetArgs(Type templateServiceType, string steeltoe, string framework, string template)
        {
            var service = BuildTemplateService(templateServiceType);
            var deps = service.GetDependencies(steeltoe, framework, template).Select(dep => dep.ShortName.ToLower());
            return from dep in deps select new object[] {service, steeltoe, framework, template, dep};
        }

        static ITemplateService BuildTemplateService(Type type)
        {
            if (type == typeof(MustacheTemplateService))
            {
                return new MustacheTemplateService(
                    TestHelper.GetConfiguration(),
                    new LoggerFactory().CreateLogger<MustacheTemplateService>());
            }

            return null;
        }
    }
}
