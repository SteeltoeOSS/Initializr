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
using Steeltoe.Initializr;
using Steeltoe.Initializr.Services;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.InitializrTests
{
    public class TemplateControllerTests : XunitLoggingBase, IClassFixture<TestWebAppFactory<Startup>>
    {
        private ILogger<MustacheTemplateService> _logger;
        private readonly TestWebAppFactory<Startup> _factory;
        private readonly HttpClient _client;

        public TemplateControllerTests(ITestOutputHelper testOutputHelper, TestWebAppFactory<Startup> factory)
            : base(testOutputHelper)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            }) ;
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            _logger = loggerFactory.CreateLogger<MustacheTemplateService>();
        }

        [Fact]
        public async void CreateZipTest()
        {
            var result = await _client.GetAsync("http://localhost/createtest");

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
          

        }
    }
}
