using System;
using System.IO;
using InitializrApi.Models;
using InitializrApi.Services;
using Xunit;
using Xunit.Abstractions;

namespace InitializrApiTests
{
    public class TemplateServiceTests: XunitLoggingBase
    {
        public TemplateServiceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
        

        [Fact]
        public void GetAvailableTemplates_returnsTemplates()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var templates = templateService.GetAvailableTemplates();
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);

            Assert.Contains(templates, x => x.ShortName == "steeltoe");
        }
        [Fact]
        public void CreateTemplate_actuators()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[]{ "Actuators"}).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);

        }
        [Fact]
        public void CreateTemplate_actuators_hystrix()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Actuators", "Hystrix" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            Assert.Contains("using Steeltoe.CircuitBreaker.Hystrix;", startUpContents);


        }
        [Fact]
        public void CreateTemplate_mysql()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "MySql" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql", startUpContents);

            Assert.Contains(".AddMySqlConnection(", startUpContents);


        }
        [Fact]
        public void CreateTemplate_actuators_cloudFoundry()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "CloudFoundry" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var programFile = Path.Combine(outFolder, "Program.cs");
            Assert.True(File.Exists(programFile));
            string programFileContents = File.ReadAllText(programFile);
            Assert.Contains("using Steeltoe.Extensions.Configuration;", programFileContents);
            Assert.Contains("using Steeltoe.Extensions.Configuration.CloudFoundry;", programFileContents);
            Assert.Contains(".UseCloudFoundryHosting(", programFileContents);
            Assert.Contains(".AddCloudFoundry", programFileContents);
        }

        [Fact]
        public void CreateTemplate_actuators_v21()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var model = new GeneratorModel
            {
                projectType = "steeltoe"

            };
            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Actuators", "SteeltoeVersion=2.1.0" }).Result;
            Console.WriteLine("outFolder " + outFolder);
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.DoesNotContain("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);", startUpContents);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);

        }
        [Fact]
        public void CreateTemplate_empty()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var model = new GeneratorModel
            {
                projectType = "steeltoe"

            };
            var outFolder = templateService.GenerateProject("steeltoe", "testProject", new string [0]).Result;
            Console.WriteLine("outFolder " + outFolder);
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.DoesNotContain("AddCloudFoundryActuators", startUpContents);

        }

    
    }
}
