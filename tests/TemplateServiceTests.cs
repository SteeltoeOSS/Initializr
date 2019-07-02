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
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.InitializrTests
{
    public class TemplateServiceTests : XunitLoggingBase
    {
        private ILogger<MustacheTemplateService> _logger;

        public TemplateServiceTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            _logger = loggerFactory.CreateLogger<MustacheTemplateService>();
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void GetAvailableTemplates_returnsTemplates(ITemplateService templateService, string templateName)
        {
            var templates = templateService.GetAvailableTemplates();
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);

            if (templateService is TemplateService)
            {
                Assert.Contains(templates, x => x.ShortName == "CSharp-WebApi-2.x");
                Assert.Contains(templates, x => x.ShortName == "steeltoe");
                Assert.Contains(templates, x => x.ShortName == "react");
            }

        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void GetDependencies(ITemplateService templateService, string templateName)
        {
            var deps = templateService.GetDependencies(null);
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "OAuthConnector");

        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void GetDependencies_WithFriendlyNames(ITemplateService templateService, string templateName)
        {
            var deps = templateService.GetDependencies();

            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "Cloud Foundry");
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_actuators(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators" },
                ProjectName = "testProject",
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Management.Hypermedia;", startUpContents);
            Assert.Contains("using Steeltoe.Management.Endpoint;", startUpContents);
            Assert.Contains("using Steeltoe.Management.CloudFoundry;", startUpContents);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_react(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                ProjectName = "testProject",
                TemplateShortName = templateName,

            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.NotEmpty(startUpContents);
          }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_discovery(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Discovery" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Discovery.Client;", startUpContents);
            Assert.Contains("services.AddDiscoveryClient(Configuration);", startUpContents);
            Assert.Contains("app.UseDiscoveryClient();", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_actuators_circuitbreakers(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators,CircuitBreaker" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            Assert.Contains("using Steeltoe.CircuitBreaker.Hystrix;", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_MySql(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MySql" },
                TemplateShortName = templateName,
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);

            Assert.Contains(".AddMySqlConnection(", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_MySql_EFCore(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MySqlEFCore" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql.EFCore;", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_postgresql(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Postgres" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql;", startUpContents);

            Assert.Contains("services.AddPostgresConnection(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_postgresEFCore(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "PostgresEFCore" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;", startUpContents);

            Assert.Contains("services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_RabbitMQ(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "RabbitMQ" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.RabbitMQ;", startUpContents);

            Assert.Contains("services.AddRabbitMQConnection(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_Redis(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Redis" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.Redis", startUpContents);
            Assert.Contains("services.AddDistributedRedisCache(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_MongoDB(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MongoDB" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MongoDb;", startUpContents);
            Assert.Contains("services.AddMongoClient(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_OauthConnector(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "OAuthConnector" },
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.OAuth;", startUpContents);
            Assert.Contains("services.AddOAuthServiceOptions(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_SqlServer(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "SQLServer" },
                ProjectName = "testProject",
                TemplateShortName = templateName,
            });

            Assert.Contains(files, file => file.Key.StartsWith("Models"));

            string fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""$(AspNetCoreVersion)"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""$(AspNetCoreVersion)"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Steeltoe.CloudFoundry.Connector.EFCore"" Version=""$(SteeltoeConnectorVersion)"" />", fileContents);

            string program = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains(@".InitializeDbContexts()", program);

            string startup = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains(@"using Steeltoe.CloudFoundry.Connector.SqlServer.EFCore;", startup);
            Assert.Contains(@"services.AddDbContext<TestContext>(options => options.UseSqlServer(Configuration));", startup);

            if (templateName != "react") //TODO: Add demo for react app
            {
                string valuesController = files.Find(x => x.Key.EndsWith("ValuesController.cs")).Value;
                Assert.Contains(@" public ValuesController(ILogger<ValuesController> logger, [FromServices] TestContext context)", valuesController);
            }
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_DynamicLogger(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "DynamicLogger" },
                ProjectName = "testProject",
                TemplateShortName = templateName,
            });

            string fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            Assert.Contains(@"<PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=""$(SteeltoeLoggingVersion)""/>", fileContents);

            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains(@"using Steeltoe.Extensions.Logging;", programFileContents);
            Assert.Contains(@"loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));", programFileContents);
            Assert.Contains(@"loggingBuilder.AddDynamicConsole();", programFileContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_actuators_cloudFoundry(ITemplateService templateService, string templateName)
        {
            Assert.NotNull(templateService);

            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "CloudFoundry" },
                TemplateShortName = templateName,
            });
            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains("using Steeltoe.Extensions.Configuration;", programFileContents);
            Assert.Contains("using Steeltoe.Extensions.Configuration.CloudFoundry;", programFileContents);
            Assert.Contains(".UseCloudFoundryHosting(", programFileContents);
            Assert.Contains(".AddCloudFoundry", programFileContents);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_actuators_v22(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators" },
                SteeltoeVersion = "2.2.0",
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            string versionProps = files.Find(x => x.Key == "versions.props").Value;

            Assert.Contains(
                @"<SteeltoeCircuitBreakerVersion>2.2.0</SteeltoeCircuitBreakerVersion>
    <SteeltoeCommonVersion>2.2.0</SteeltoeCommonVersion>
    <SteeltoeConfigVersion>2.2.0</SteeltoeConfigVersion>
    <SteeltoeConnectorVersion>2.2.0</SteeltoeConnectorVersion>
    <SteeltoeDiscoveryVersion>2.2.0</SteeltoeDiscoveryVersion>
    <SteeltoeLoggingVersion>2.2.0</SteeltoeLoggingVersion>
    <SteeltoeManagementVersion>2.2.0</SteeltoeManagementVersion>
    <SteeltoeSecurityVersion>2.2.0</SteeltoeSecurityVersion>", versionProps);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_actuators_23rc1(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators" },
                SteeltoeVersion = "2.3.0-rc1",
                TemplateShortName = templateName,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            string versionProps = files.Find(x => x.Key == "versions.props").Value;

            Assert.Contains(
                @"<SteeltoeCircuitBreakerVersion>2.3.0-rc1</SteeltoeCircuitBreakerVersion>
    <SteeltoeCommonVersion>2.3.0-rc1</SteeltoeCommonVersion>
    <SteeltoeConfigVersion>2.3.0-rc1</SteeltoeConfigVersion>
    <SteeltoeConnectorVersion>2.3.0-rc1</SteeltoeConnectorVersion>
    <SteeltoeDiscoveryVersion>2.3.0-rc1</SteeltoeDiscoveryVersion>
    <SteeltoeLoggingVersion>2.3.0-rc1</SteeltoeLoggingVersion>
    <SteeltoeManagementVersion>2.3.0-rc1</SteeltoeManagementVersion>
    <SteeltoeSecurityVersion>2.3.0-rc1</SteeltoeSecurityVersion>", versionProps);
        }
        ////[Fact]
        ////public void CreateTemplate_actuators_v3()
        ////{
        ////    var templateService = new MustacheTemplateService(_logger);

        ////    Assert.NotNull(templateService);

        ////    var outFolder = templateService.GenerateProject("steeltoe", "testProject", new[] { "Actuators" }).Result;
        ////    Console.WriteLine("outFolder " + outFolder);
        ////    Assert.NotNull(outFolder);
        ////    Assert.True(Directory.Exists(outFolder));
        ////    var startupPath = Path.Combine(outFolder, "Startup.cs");
        ////    Assert.True(File.Exists(startupPath));
        ////    string startUpContents = File.ReadAllText(startupPath);
        ////    Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
        ////    Assert.DoesNotContain("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        ////}

        [Theory]
        [ClassData(typeof(TestData))]
        public void CreateTemplate_empty(ITemplateService templateService, string templateName)
        {
            Assert.NotNull(templateService);

            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                TemplateShortName = templateName,
                ProjectName = "Foo.Bar",
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.DoesNotContain(files, file => file.Key.StartsWith("Models"));
            Assert.DoesNotContain("AddCloudFoundryActuators", startUpContents);

            string dockerFile = files.Find(x => x.Key == "Dockerfile").Value;
            Assert.NotNull(dockerFile);

            Assert.Contains("Foo.Bar.dll", dockerFile);
            Assert.Contains("Foo.Bar.csproj", dockerFile);

            foreach (var file in files)
            {
                Assert.DoesNotContain("{{", file.Value);
                Assert.DoesNotContain("}}", file.Value);
            }
        }
    }
}
