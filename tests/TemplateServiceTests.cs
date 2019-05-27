using System;
using InitializrApi.Models;
using InitializrApi.Services;
using Xunit;
using Xunit.Abstractions;

namespace InitializrApiTests
{
    public class TemplateServiceTests: XunitLoggingBase
    {
        ITestOutputHelper _testOutputHelper;
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
            var zipPath = templateService.GenerateProject();
            Assert.NotNull(zipPath);

        }

        [Fact]
        public void WriteToConsole()
        {
            Console.WriteLine("test");
            
          //  Output.WriteLine("test itestoutputhelper");
        }

      
    }
}
