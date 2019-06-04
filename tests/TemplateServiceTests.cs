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
        public void CreateTemplate()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var model = new GeneratorModel
            {
                projectType = "steeltoe"

            };
            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[]{ "Actuators"}).Result;
            Console.WriteLine("outFolder " + outFolder);
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);", startUpContents);

        }
        [Fact]
        public void CreateTemplate_NoActuators()
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

        [Fact]
        public void WriteToConsole()
        {
            Console.WriteLine("test");
            
          //  Output.WriteLine("test itestoutputhelper");
        }

      
    }
}
