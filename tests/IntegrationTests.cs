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


                var archive = await templateService.GenerateProjectArchiveAsync(new Models.GeneratorModel()
                {
                    // Todo : fix this
                    Dependencies = new[] {dep.ShortName},
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