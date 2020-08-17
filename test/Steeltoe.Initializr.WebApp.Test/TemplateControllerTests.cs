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

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.TemplateEngine.Models;
using Steeltoe.Initializr.TemplateEngine.Services;
using Steeltoe.Initializr.TemplateEngine.Services.Mustache;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Steeltoe.Initializr.TemplateEngine;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.Initializr.WebApp.Test
{
    /// <summary>
    /// Controller Tests for multiple templateService implementations.
    /// </summary>
    public class TemplateControllerTests : XunitLoggingBase, IClassFixture<TestWebAppFactory<Startup>>
    {
        private readonly HttpClient _client;
        private ILogger<MustacheTemplateService> _logger;

        public TemplateControllerTests(ITestOutputHelper testOutputHelper, TestWebAppFactory<Startup> factory)
            : base(testOutputHelper)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            _logger = loggerFactory.CreateLogger<MustacheTemplateService>();
        }

        [Fact]
        public async void GetStarterZipTest()
        {
            var result = await _client.GetAsync("https://localhost/starter.zip?ProjectName=TestCompany.TestProject&Dependencies=Actuator,MySql&Description=Test%20Description&SteeltoeVersion=2.4.4&DotNetFramework=netcoreapp2.1&Template=webapi");

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            Dictionary<string, string> files = new Dictionary<string, string>();
            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fileStream = entry.Open();
                        files.Add(entry.Name, new StreamReader(fileStream).ReadToEnd());
                    }
                }
            }

            Assert.True(files.Count > 0);
            Assert.Contains("Program.cs", files.Keys);
            Assert.Contains("TestCompany.TestProject", files["Program.cs"]);
        }

        [Fact]
        public async void GetStarterZipValidationTest()
        {
            var result = await _client.GetAsync("https://localhost/starter.zip?ProjectName=123.TestProject");

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

            var responseString = await result.Content.ReadAsStringAsync();
            Assert.Contains(@"""ProjectName"":[""ProjectName must be a valid C# Identifier""]", responseString);
        }

        [Fact]
        public async void PostStarterZipValidationTest()
        {
            var model = new GeneratorModel()
            {
                Dependencies = "Actuator,MySql",
                Description = "TestDescription",
                ProjectName = "123.TestProject",
                SteeltoeVersion = "2.4.4",
                TargetFramework = "netcoreapp2.1",
                Template = "Steeltoe-WebApi",
            };

            var props = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var kvps = props.Select(prop => new KeyValuePair<string, string>(prop.Name, prop.GetValue(model).ToString())).ToArray();

            var formContent = new FormUrlEncodedContent(kvps);

            var result = await _client.PostAsync("https://localhost/starter.zip", formContent);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

            var responseString = await result.Content.ReadAsStringAsync();
            Assert.Contains(@"""ProjectName"":[""ProjectName must be a valid C# Identifier""]", responseString);
        }

        [Fact]
        public async void PostStarterZipTest()
        {
            var model = new GeneratorModel()
            {
                Dependencies = "Actuator,MySql",
                Description = "TestDescription",
                ProjectName = "TestCompany.TestProject",
                SteeltoeVersion = Constants.Steeltoe24,
                TargetFramework = Constants.NetCoreApp21,
                Template = Constants.WebApi,
            };

            var props = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var kvps = props.Select(prop => new KeyValuePair<string, string>(prop.Name, prop.GetValue(model).ToString())).ToArray();

            var formContent = new FormUrlEncodedContent(kvps);

            var result = await _client.PostAsync("https://localhost/starter.zip", formContent);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            Dictionary<string, string> files = new Dictionary<string, string>();
            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fileStream = entry.Open();
                        files.Add(entry.Name, new StreamReader(fileStream).ReadToEnd());
                    }
                }
            }

            Assert.True(files.Count > 0);
            Assert.Contains("Program.cs", files.Keys);
            Assert.Contains("TestCompany.TestProject", files["Program.cs"]);
        }
    }
}
