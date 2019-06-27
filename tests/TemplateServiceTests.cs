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
        [ClassData(typeof(TemplateServiceImplementations))]
        public void GetAvailableTemplates_returnsTemplates(ITemplateService templateService)
        {
            var templates = templateService.GetAvailableTemplates();
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);

            if (templateService is TemplateService)
            {
                Assert.Contains(templates, x => x.ShortName == "steeltoe");
                Assert.Contains(templates, x => x.ShortName == "steeltoe2");
            }

        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void GetDependencies(ITemplateService templateService)
        {
            var deps = templateService.GetDependencies(null);
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "OAuthConnector");

        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void GetDependencies_WithFriendlyNames(ITemplateService templateService)
        {
            var deps = templateService.GetDependencies();
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "Cloud Foundry");
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_actuators(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators" },
                ProjectName = "testProject",
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Management.Hypermedia;", startUpContents);
            Assert.Contains("using Steeltoe.Management.Endpoint;", startUpContents);
            Assert.Contains("using Steeltoe.Management.CloudFoundry;", startUpContents);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_discovery(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Discovery" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Discovery.Client;", startUpContents);
            Assert.Contains("services.AddDiscoveryClient(Configuration);", startUpContents);
            Assert.Contains("app.UseDiscoveryClient();", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_actuators_circuitbreakers(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators,CircuitBreaker" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            Assert.Contains("using Steeltoe.CircuitBreaker.Hystrix;", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_MySql(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MySql" },
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);

            Assert.Contains(".AddMySqlConnection(", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_MySql_EFCore(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MySqlEFCore" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql.EFCore;", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_postgresql(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Postgres" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql;", startUpContents);

            Assert.Contains("services.AddPostgresConnection(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_postgresEFCore(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "PostgresEFCore" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;", startUpContents);

            Assert.Contains("services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_RabbitMQ(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "RabbitMQ" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.RabbitMQ;", startUpContents);

            Assert.Contains("services.AddRabbitMQConnection(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_Redis(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Redis" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.Redis", startUpContents);
            Assert.Contains("services.AddDistributedRedisCache(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_MongoDB(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MongoDB" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MongoDb;", startUpContents);
            Assert.Contains("services.AddMongoClient(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_OauthConnector(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "OAuthConnector" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.OAuth;", startUpContents);
            Assert.Contains("services.AddOAuthServiceOptions(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_SqlServer(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "SQLServer" },
                ProjectName = "testProject",
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

            string valuesController = files.Find(x => x.Key.EndsWith("ValuesController.cs")).Value;
            Assert.Contains(@" public ValuesController(ILogger<ValuesController> logger, [FromServices] TestContext context)", valuesController);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_DynamicLogger(ITemplateService templateService)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "DynamicLogger" },
                ProjectName = "testProject",
            });

            string fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            Assert.Contains(@"<PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=""$(SteeltoeLoggingVersion)""/>", fileContents);

            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains(@"using Steeltoe.Extensions.Logging;", programFileContents);
            Assert.Contains(@"loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));", programFileContents);
            Assert.Contains(@"loggingBuilder.AddDynamicConsole();", programFileContents);
        }

        [Theory]
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_actuators_cloudFoundry(ITemplateService templateService)
        {
            Assert.NotNull(templateService);

            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "CloudFoundry" },
            });
            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains("using Steeltoe.Extensions.Configuration;", programFileContents);
            Assert.Contains("using Steeltoe.Extensions.Configuration.CloudFoundry;", programFileContents);
            Assert.Contains(".UseCloudFoundryHosting(", programFileContents);
            Assert.Contains(".AddCloudFoundry", programFileContents);
        }

        //[Fact]
        //public void CreateTemplate_actuators_v21()
        //{
        //    var templateService = new MustacheTemplateService(_logger);

        //    Assert.NotNull(templateService);

        //    var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Actuators", "SteeltoeVersion=2.1.0" }).Result;
        //    Console.WriteLine("outFolder " + outFolder);
        //    Assert.NotNull(outFolder);
        //    Assert.True(Directory.Exists(outFolder));
        //    var startupPath = Path.Combine(outFolder, "Startup.cs");
        //    Assert.True(File.Exists(startupPath));
        //    string startUpContents = File.ReadAllText(startupPath);
        //    Assert.DoesNotContain("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
        //    Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        // }

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
        [ClassData(typeof(TemplateServiceImplementations))]
        public void CreateTemplate_empty(ITemplateService templateService)
        {
            Assert.NotNull(templateService);

            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel());
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.DoesNotContain(files, file => file.Key.StartsWith("Models"));
            Assert.DoesNotContain("AddCloudFoundryActuators", startUpContents);

            foreach (var file in files)
            {
                Assert.DoesNotContain("{{", file.Value);
                Assert.DoesNotContain("}}", file.Value);
            }
        }
    }
}
