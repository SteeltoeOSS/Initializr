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

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.TemplateEngine.Models;
using Steeltoe.Initializr.TemplateEngine.Services;
using Steeltoe.Initializr.TemplateEngine.Services.DotNetTemplateEngine;
using Steeltoe.Initializr.TemplateEngine.Services.Mustache;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.Initializr.TemplateEngine.Test
{
    public class TemplateServiceTests : XunitLoggingBase
    {
        private readonly LoggerFactory _loggerFactory;

        public TemplateServiceTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
        }

        [Fact]
        public void GetAvailableTemplates_returnsTemplates()
        {
            var templates = new DotnetTemplateService(_loggerFactory.CreateLogger<DotnetTemplateService>())
                .GetAvailableTemplates();
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);

            Assert.Contains(templates, x => x.ShortName == "Steeltoe-WebApi" && x.DotnetTemplateVersion == DotnetTemplateVersion.V2);
            Assert.Contains(templates, x => x.ShortName == "Steeltoe-WebApi" && x.DotnetTemplateVersion == DotnetTemplateVersion.V3);
            Assert.Contains(templates, x => x.ShortName == "Steeltoe-React" && x.DotnetTemplateVersion == DotnetTemplateVersion.V2);
            Assert.Contains(templates, x => x.ShortName == "Steeltoe-React" && x.DotnetTemplateVersion == DotnetTemplateVersion.V3);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void GetDependencies(ITemplateService templateService, string templateName, DotnetTemplateVersion dotnetTemplateVersion)
        {
            var deps = templateService.GetDependencies(templateName, dotnetTemplateVersion);
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "OAuthConnector");
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void GetDependencies_WithFriendlyNames(ITemplateService templateService, string templateName, DotnetTemplateVersion dotnetTemplateVersion)
        {
            var deps = templateService.GetDependencies(templateName, dotnetTemplateVersion);

            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "Cloud Foundry");
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                ProjectName = "testProject",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Management.Hypermedia;", startUpContents);
            Assert.Contains("using Steeltoe.Management.Endpoint;", startUpContents);
            Assert.Contains("using Steeltoe.Management.CloudFoundry;", startUpContents);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_react(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                ProjectName = "testProject",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.NotEmpty(startUpContents);
          }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_discovery(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Discovery",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Discovery.Client;", startUpContents);
            Assert.Contains("services.AddDiscoveryClient(Configuration);", startUpContents);
            Assert.Contains("app.UseDiscoveryClient();", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_circuitbreakers(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators,CircuitBreaker",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
            Assert.Contains("using Steeltoe.CircuitBreaker.Hystrix;", startUpContents);

            Assert.Contains(files, file => file.Key.EndsWith("MyCircuitBreakerCommand.cs"));

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains(@"MyCircuitBreakerCommand cb = new MyCircuitBreakerCommand(""ThisIsMyBreaker"");", valuesController);

            string appSettings = files.Find(x => x.Key == "appsettings.json").Value;
            Assert.DoesNotContain("#if", appSettings);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_MySql(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "MySql",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);

            Assert.Contains(".AddMySqlConnection(", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using MySql.Data.MySqlClient;", valuesController);
            Assert.Contains("using System.Data", valuesController);

            Assert.Contains(
                @"private readonly MySqlConnection _dbConnection;
        public ValuesController([FromServices] MySqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }", valuesController);

            Assert.Contains(@"DataTable dt = _dbConnection.GetSchema(""Tables"");", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_MySql_EFCore(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "MySqlEFCore",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql.EFCore;", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_postgresql(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Postgres",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql;", startUpContents);
            Assert.Contains("services.AddPostgresConnection(Configuration);", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Npgsql;", valuesController);
            Assert.Contains("using System.Data", valuesController);

            Assert.Contains(@"public ValuesController([FromServices] NpgsqlConnection dbConnection)", valuesController);
            Assert.Contains(@"DataTable dt = _dbConnection.GetSchema(""Databases"");", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_ConfigServer(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "ConfigServer",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            Assert.DoesNotContain(files, file => file.Key.EndsWith("SampleData.cs"));
            Assert.Contains(files, file => file.Key.EndsWith("ValuesController.cs"));

            string programContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains("using Steeltoe.Extensions.Configuration.ConfigServer;", programContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Microsoft.Extensions.Configuration;", valuesController);

            Assert.Contains(@"public ValuesController(IConfiguration config)", valuesController);
            Assert.Contains(@"_config[""Value1""];", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_Randomvalue(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "RandomValueConfig",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            Assert.Contains(files, file => file.Key.EndsWith("ValuesController.cs"));

            string programContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains("using Steeltoe.Extensions.Configuration.RandomValue;", programContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Microsoft.Extensions.Configuration;", valuesController);

            Assert.Contains(@"public ValuesController(IConfiguration config)", valuesController);
            Assert.Contains(@"_config[""random:int""];", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_Cloudfoundry(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "CloudFoundry",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string programContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains(".UseCloudFoundryHosting()", programContents);
            Assert.Contains(".AddCloudFoundry()", programContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Steeltoe.Extensions.Configuration.CloudFoundry;", valuesController);
            Assert.Contains("using Microsoft.Extensions.Options;", valuesController);

            Assert.Contains(@"public ValuesController(ILogger<ValuesController> logger, IOptions<CloudFoundryApplicationOptions> appOptions, IOptions<CloudFoundryServicesOptions> serviceOptions)", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_Placeholderconfig(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "PlaceholderConfig",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            Assert.DoesNotContain(files, file => file.Key.EndsWith("SampleData.cs"));
            Assert.Contains(files, file => file.Key.EndsWith("ValuesController.cs"));

            string programContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains("using Steeltoe.Extensions.Configuration.PlaceholderCore;", programContents);

            string valuesController =
                files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Microsoft.Extensions.Configuration;", valuesController);

            Assert.Contains(@"public ValuesController(IConfiguration config)", valuesController);
            Assert.Contains(@"_config[""ResolvedPlaceholderFromEnvVariables""];", valuesController);

            string appSettings = files.Find(x => x.Key == $"appsettings.json").Value;
            Assert.Contains("\"ResolvedPlaceholderFromEnvVariables\": \"${PATH?NotFound}\"", appSettings);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_postgresEFCore(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "PostgresEFCore",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;", startUpContents);

            Assert.Contains("services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_RabbitMQ(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "RabbitMQ",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.RabbitMQ;", startUpContents);
            Assert.Contains("services.AddRabbitMQConnection(Configuration);", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains(
                @"using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;", valuesController);

            Assert.Contains(@"public ValuesController(ILogger<ValuesController> logger, [FromServices] ConnectionFactory factory)", valuesController);
            Assert.Contains(
                @"channel.BasicPublish(exchange: """",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_Redis(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Redis",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.Redis", startUpContents);
            Assert.Contains("services.AddDistributedRedisCache(Configuration);", startUpContents);
            Assert.Contains("// services.AddRedisConnectionMultiplexer(Configuration);", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Microsoft.Extensions.Caching.Distributed;", valuesController);

            Assert.Contains(@" public ValuesController(IDistributedCache cache)", valuesController);
            Assert.Contains(@"await _cache.SetStringAsync(""MyValue1"", ""123"");", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_MongoDB(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "MongoDB",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MongoDb;", startUpContents);
            Assert.Contains("services.AddMongoClient(Configuration);", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using MongoDB.Driver;", valuesController);
            Assert.Contains("using System.Data", valuesController);

            Assert.Contains(@"public ValuesController(IMongoClient mongoClient, MongoUrl mongoUrl)", valuesController);
            Assert.Contains(@"_mongoClient.ListDatabaseNames().ToList();", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_OauthConnector(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "OAuthConnector",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.OAuth;", startUpContents);
            Assert.Contains("services.AddOAuthServiceOptions(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_SqlServer(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var steeltoeVersion = "2.4.3";

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "SQLServer",
                ProjectName = "testProject",
                TemplateShortName = templateName,
                SteeltoeVersion = steeltoeVersion,
                DotnetTemplateVersion = version,
            });

            var fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            var aspnetCoreVersion = version == DotnetTemplateVersion.V3 ? "3.1.0" : "2.2.0";

            Assert.Contains($@"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""{aspnetCoreVersion}"" />", fileContents);

            var startup = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains(@"using Steeltoe.CloudFoundry.Connector.SqlServer;", startup);
            Assert.Contains(@"services.AddSqlServerConnection(Configuration);", startup);

            if (!templateName.Contains("React"))
            { // TODO: Add demo for react app
                var valuesController = files.Find(x => x.Key.EndsWith("ValuesController.cs")).Value;
                Assert.Contains(@" public ValuesController([FromServices] SqlConnection dbConnection)", valuesController);
            }
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_DynamicLogger(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "DynamicLogger",
                ProjectName = "testProject",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            string fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            Assert.Contains(@"<PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=", fileContents);

            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains(@"using Steeltoe.Extensions.Logging;", programFileContents);
            Assert.Contains(@"loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));", programFileContents);
            Assert.Contains(@"loggingBuilder.AddDynamicConsole();", programFileContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_cloudFoundry(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            Assert.NotNull(templateService);

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "CloudFoundry",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });
            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains("using Steeltoe.Extensions.Configuration;", programFileContents);
            Assert.Contains("using Steeltoe.Extensions.Configuration.CloudFoundry;", programFileContents);
            Assert.Contains(".UseCloudFoundryHosting(", programFileContents);
            Assert.Contains(".AddCloudFoundry", programFileContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_v22(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                SteeltoeVersion = "2.4.3",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_23(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                SteeltoeVersion = "2.4.3",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_24(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                SteeltoeVersion = "2.4.3",
                TemplateShortName = templateName,
                DotnetTemplateVersion = version,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplateNames))]
        public void CreateTemplate_v3_invalid(ITemplateService templateService, string templateName)
        {
            Assert.ThrowsAsync<InvalidDataException>(async () =>
            {
                var files = await templateService.GenerateProjectFiles(new GeneratorModel()
                {
                    Dependencies = "Actuators",
                    SteeltoeVersion = "2.4.2",
                    TemplateShortName = templateName,
                    DotnetTemplateVersion = DotnetTemplateVersion.V3,
                });
            });
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_empty(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            Assert.NotNull(templateService);

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                TemplateShortName = templateName,
                ProjectName = "Foo.Bar",
                DotnetTemplateVersion = version,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.DoesNotContain(files, file => file.Key.StartsWith("Models"));
            Assert.DoesNotContain(files, file => file.Key.EndsWith("MyCircuitBreakerCommand.cs"));
            Assert.DoesNotContain("AddCloudFoundryActuators", startUpContents);

            var projectFile = files.Find(x => x.Key == "Foo.Bar.csproj").Value;
            var targetFramework = version == DotnetTemplateVersion.V3 ? "netcoreapp3.1" : "netcoreapp2.2";
            Assert.Contains($"<TargetFramework>{targetFramework}</TargetFramework>", projectFile);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_targetVersion21(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            if (version == DotnetTemplateVersion.V3)
            {
                return;
            }

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators,SQLServer",
                TemplateShortName = templateName,
                ProjectName = "Foo.Bar",
                TargetFrameworkVersion = "netcoreapp2.1",
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("services.AddMvc();", startUpContents);

            string projectFile = files.Find(x => x.Key == "Foo.Bar.csproj").Value;
            Assert.Contains("<TargetFramework>netcoreapp2.1</TargetFramework>", projectFile);
         }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_targetVersion22(ITemplateService templateService, string templateName, DotnetTemplateVersion version)
        {
            if (version == DotnetTemplateVersion.V3)
            {
                return;
            }

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                TemplateShortName = templateName,
                ProjectName = "Foo.Bar",
                TargetFrameworkVersion = "netcoreapp2.2",
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);", startUpContents);

            var projectFile = files.Find(x => x.Key == "Foo.Bar.csproj").Value;
            Assert.Contains("<TargetFramework>netcoreapp2.2</TargetFramework>", projectFile);
        }
    }
}
