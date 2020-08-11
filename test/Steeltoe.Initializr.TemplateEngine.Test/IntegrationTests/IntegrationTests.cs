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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Steeltoe.Initializr.TemplateEngine.Models;
using Steeltoe.Initializr.TemplateEngine.Services;
using Steeltoe.Initializr.TemplateEngine.Services.DotNetTemplateEngine;
using Steeltoe.Initializr.TemplateEngine.Services.Mustache;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.Initializr.TemplateEngine.Test.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class IntegrationTests : XunitLoggingBase
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly LoggerFactory _xUnitLoggerFactory;
        private readonly LoggerFactory _loggerFactory;

        public IntegrationTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _xUnitLoggerFactory = new LoggerFactory();
            _xUnitLoggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            _loggerFactory = new LoggerFactory();
        }

        public static IEnumerable<object[]> GetAllCombinations(Type templateServiceType, string templateName, DotnetTemplateVersion version, int take)
        {
            ITemplateService templateService;
            if (templateServiceType == typeof(MustacheTemplateService))
            {
                templateService = new MustacheTemplateService(
                    TestHelper.GetConfiguration(),
                    new LoggerFactory().CreateLogger<MustacheTemplateService>());
            }
            else
            {
                templateService = new DotnetTemplateService(
                    TestHelper.GetConfiguration(),
                    new MemoryCache(new MemoryCacheOptions()),
                    new LoggerFactory().CreateLogger<DotnetTemplateService>());
            }

            var dependencies = templateService.GetDependencies(templateName, version);
            return from deps in GetCombinations(dependencies.Select(d => d.ShortName), take)
                select new object[] { templateService, templateName, version, string.Join(",", deps) };
        }

        [Theory]
        [MemberData(nameof(GetAllCombinations), typeof(MustacheTemplateService), "Steeltoe-WebApi", DotnetTemplateVersion.V2, 1,  DisableDiscoveryEnumeration = true)]
        public async Task CreateTemplate_Mustache_WebApi_V2_OneAtaTime_Test(ITemplateService templateService, string templateName, DotnetTemplateVersion version, string depString)
        {
            _testOutputHelper.WriteLine(depString);
            await CreateTemplate_Test(templateService, templateName, version, depString);
        }

        [Theory]
        [MemberData(nameof(GetAllCombinations), typeof(MustacheTemplateService), "Steeltoe-WebApi", DotnetTemplateVersion.V2, int.MaxValue,  DisableDiscoveryEnumeration = true)]
        public async Task CreateTemplate_Mustache_WebApi_V2_All_Test(ITemplateService templateService, string templateName, DotnetTemplateVersion version, string depString)
        {
            _testOutputHelper.WriteLine(depString);
            await CreateTemplate_Test(templateService, templateName, version, depString);
        }

        // [Theory]
        // [MemberData(nameof(GetAllCombinations), typeof(TemplateService), "Steeltoe-WebApi", TemplateVersion.V2, 1, DisableDiscoveryEnumeration = true)]
        // public async Task CreateTemplate_Dotnet_WebApi_V2_OneAtaTime_Test(ITemplateService templateService, string templateName, TemplateVersion version, string depString)
        // {
            // _testOutputHelper.WriteLine(depString);
            // await CreateTemplate_Test(templateService, templateName, version, depString);
        // }

        // [Theory]
        // [MemberData(nameof(GetAllCombinations), typeof(TemplateService), "Steeltoe-WebApi", TemplateVersion.V2, int.MaxValue, DisableDiscoveryEnumeration = true)]
        // public async Task CreateTemplate_Dotnet_WebApi_V2_All_Test(ITemplateService templateService, string templateName, TemplateVersion version, string depString)
        // {
            // _testOutputHelper.WriteLine(depString);
            // await CreateTemplate_Test(templateService, templateName, version, depString);
        // }

        private static IEnumerable<List<string>> GetCombinations(IEnumerable<string> deps, int take)
        {
            var returnValue = new List<List<string>>();

            var depArray = deps as string[] ?? deps.ToArray();
            var length = depArray.Length;
            var combinations = Math.Pow(2, length);

            for (var i = 0; i < combinations; i++)
            {
                var combinationDeps = new List<string>();
                for (var bit = 0; bit <= length; bit++)
                {
                    // right shift by _bit_ bits and check if set
                    if (((i >> bit) & 1) == 1)
                    {
                        combinationDeps.Add(depArray[bit]);
                    }
                }

                if (combinationDeps.Count == take || (take == int.MaxValue && combinationDeps.Count == depArray.Length))
                {
                    returnValue.Add(combinationDeps);
                }
            }

            return returnValue;
        }

        /*
        [Theory]
        [InlineData("Actuators")]
        [InlineData("CircuitBreaker")]
        [InlineData("CloudFoundry")]
        [InlineData("Discovery")]
        [InlineData("DynamicLogger")]
        [InlineData("MongoDB")]
        [InlineData("MySql")]
        [InlineData("MySqlEFCore")]
        [InlineData("OAuthConnector")]
        [InlineData("Postgres")]
        [InlineData("PostgresEFCore")]
        [InlineData("RabbitMQ")]
        [InlineData("Redis")]
        [InlineData("SQLServer")]
        [InlineData("ConfigServer")]
        [InlineData("PlaceholderConfig")]
        [InlineData("RandomValueConfig")]
        [InlineData("Dockerfile")]
        public async Task TestBuildSteeltoe2(string dependency)
        {
            var templateService = new MustacheTemplateService(new NullLogger<MustacheTemplateService>());
            await CreateTemplate_Test(templateService, "Steeltoe-WebApi", DotnetTemplateVersion.V2, dependency);
        }
        */

        /*
        [Theory]
        [InlineData("Actuators")]
        [InlineData("CircuitBreaker")]
        [InlineData("AzureSpringCloud")]
        [InlineData("CloudFoundry")]
        [InlineData("Discovery")]
        [InlineData("DynamicLogger")]
        [InlineData("MongoDB")]
        [InlineData("MySql")]
        [InlineData("MySqlEFCore")]
        [InlineData("OAuthConnector")]
        [InlineData("Postgres")]
        [InlineData("PostgresEFCore")]
        [InlineData("RabbitMQ")]
        [InlineData("Redis")]
        [InlineData("SQLServer")]
        [InlineData("ConfigServer")]
        [InlineData("PlaceholderConfig")]
        [InlineData("RandomValueConfig")]
        [InlineData("Dockerfile")]
        public async Task TestBuildSteeltoe3(string dependency)
        {
            var templateService = new MustacheTemplateService(new NullLogger<MustacheTemplateService>());
            await CreateTemplate_Test(templateService, "Steeltoe-WebApi", DotnetTemplateVersion.V3, dependency, "3.0.0-rc1");
        }
        */

        private async Task CreateTemplate_Test(ITemplateService service, string template, DotnetTemplateVersion dotnet, string dependency, string steeltoe = "2.4.4")
        {
            _testOutputHelper.WriteLine($"testing Steeltoe {steeltoe} with {dependency}");

            var archive = await service.GenerateProjectArchiveAsync(new GeneratorModel()
            {
                Dependencies = dependency,
                TemplateShortName = template,
                ProjectName = "Foo.Bar",
                TemplateVersion = dotnet,
                SteeltoeVersion = steeltoe,
            });

            var zip = new ZipArchive(new MemoryStream(archive));
            var dirName = Path.GetTempPath() + Path.DirectorySeparatorChar + Guid.NewGuid();
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
    }
}
