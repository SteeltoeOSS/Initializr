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
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.TemplateEngine.Models;
using Steeltoe.Initializr.TemplateEngine.Services;
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

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void GetDependencies(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var deps = templateService.GetDependencies(steeltoe, framework, template);
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "OAuthConnector");
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void GetDependencies_WithFriendlyNames(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var deps = templateService.GetDependencies(steeltoe, framework, template);

            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "Cloud Foundry");
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                ProjectName = "testProject",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Management.CloudFoundry;", startUpContents);
            if (!framework.Equals("netcoreapp3.1"))
            {
                Assert.Contains("using Steeltoe.Management.Endpoint;", startUpContents);
                Assert.Contains("using Steeltoe.Management.Hypermedia;", startUpContents);
            }
            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_react(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                ProjectName = "testProject",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.NotEmpty(startUpContents);
          }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_discovery(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Discovery",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Discovery.Client;", startUpContents);
            Assert.Contains("services.AddDiscoveryClient(Configuration);", startUpContents);
            Assert.Contains("app.UseDiscoveryClient();", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_circuitbreakers(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators,CircuitBreaker",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
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
        public async Task CreateTemplate_MySql(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "MySql",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.MySql;", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);
            }

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
        public async Task CreateTemplate_MySql_EFCore(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "MySqlEFCore",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.MySql;", startUpContents);
                Assert.Contains("using Steeltoe.Connector.MySql.EFCore;", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql.EFCore;", startUpContents);
            }
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_postgresql(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Postgres",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.PostgreSql;", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql;", startUpContents);
            }
            Assert.Contains("services.AddPostgresConnection(Configuration);", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Npgsql;", valuesController);
            Assert.Contains("using System.Data", valuesController);

            Assert.Contains(@"public ValuesController([FromServices] NpgsqlConnection dbConnection)", valuesController);
            Assert.Contains(@"DataTable dt = _dbConnection.GetSchema(""Databases"");", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_ConfigServer(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "ConfigServer",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
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
        public async Task CreateTemplate_Randomvalue(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "RandomValueConfig",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
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
        public async Task CreateTemplate_Cloudfoundry(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "CloudFoundry",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string programContents = files.Find(x => x.Key == "Program.cs").Value;

            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains(".UseCloudHosting()", programContents);
                Assert.Contains(".AddCloudFoundryConfiguration()", programContents);
            }
            else
            {
                Assert.Contains(".UseCloudFoundryHosting()", programContents);
                Assert.Contains(".AddCloudFoundry()", programContents);
            }

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Microsoft.Extensions.Options;", valuesController);

            Assert.Contains(@"public ValuesController(ILogger<ValuesController> logger, IOptions<CloudFoundryApplicationOptions> appOptions, IOptions<CloudFoundryServicesOptions> serviceOptions)", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_Placeholderconfig(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var configuration = TestHelper.GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<MustacheTemplateService>();

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "PlaceholderConfig",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            Assert.DoesNotContain(files, file => file.Key.EndsWith("SampleData.cs"));
            Assert.Contains(files, file => file.Key.EndsWith("ValuesController.cs"));

            string programContents = files.Find(x => x.Key == "Program.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Extensions.Configuration.Placeholder;", programContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.Extensions.Configuration.PlaceholderCore;", programContents);
            }

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
        public async Task CreateTemplate_postgresEFCore(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "PostgresEFCore",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.PostgreSql.EFCore;", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;", startUpContents);
            }

            Assert.Contains("services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_RabbitMQ(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "RabbitMQ",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.RabbitMQ;", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.RabbitMQ;", startUpContents);
            }
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
        public async Task CreateTemplate_Redis(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Redis",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.Redis", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.Redis", startUpContents);
            }
            Assert.Contains("services.AddDistributedRedisCache(Configuration);", startUpContents);
            Assert.Contains("// services.AddRedisConnectionMultiplexer(Configuration);", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using Microsoft.Extensions.Caching.Distributed;", valuesController);

            Assert.Contains(@" public ValuesController(IDistributedCache cache)", valuesController);
            Assert.Contains(@"await _cache.SetStringAsync(""MyValue1"", ""123"");", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_MongoDB(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "MongoDB",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.MongoDb;", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.MongoDb;", startUpContents);
            }
            Assert.Contains("services.AddMongoClient(Configuration);", startUpContents);

            string valuesController = files.Find(x => x.Key == $"Controllers{Path.DirectorySeparatorChar}ValuesController.cs").Value;
            Assert.Contains("using MongoDB.Driver;", valuesController);
            Assert.Contains("using System.Data", valuesController);

            Assert.Contains(@"public ValuesController(IMongoClient mongoClient, MongoUrl mongoUrl)", valuesController);
            Assert.Contains(@"_mongoClient.ListDatabaseNames().ToList();", valuesController);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_OauthConnector(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "OAuthConnector",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Connector.OAuth;", startUpContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.CloudFoundry.Connector.OAuth;", startUpContents);
            }
            Assert.Contains("services.AddOAuthServiceOptions(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_SqlServer(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "SQLServer",
                ProjectName = "testProject",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            var fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            var aspnetCoreVersion = framework.Equals("netcoreapp3.1") ? "3.1.0" : "2.1.1";
            var startup = files.Find(x => x.Key == "Startup.cs").Value;

            if (framework.Equals(Constants.NetCoreApp21))
            {
                Assert.Contains($@"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""{aspnetCoreVersion}"" />", fileContents);
                Assert.Contains(@"using Steeltoe.CloudFoundry.Connector.SqlServer;", startup);
                Assert.Contains(@"services.AddSqlServerConnection(Configuration);", startup);
            }
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_DynamicLogger(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "DynamicLogger",
                ProjectName = "testProject",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            string fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains(@"<PackageReference Include=""Steeltoe.Extensions.Logging.DynamicSerilogCore"" Version=", fileContents);
                Assert.Contains(@"using Steeltoe.Extensions.Logging.DynamicSerilog", programFileContents);
                Assert.Contains(@"ConfigureLogging((context, builder) => builder.AddSerilogDynamicConsole())", programFileContents);
            }
            else
            {
                Assert.Contains(@"<PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=", fileContents);
                Assert.Contains(@"using Steeltoe.Extensions.Logging;", programFileContents);
                Assert.Contains(@"loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));", programFileContents);
                Assert.Contains(@"loggingBuilder.AddDynamicConsole();", programFileContents);
            }
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_cloudFoundry(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            Assert.NotNull(templateService);

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "CloudFoundry",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });
            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            if (steeltoe.Equals(Constants.Steeltoe30))
            {
                Assert.Contains("using Steeltoe.Common.Hosting;", programFileContents);
                Assert.Contains(".UseCloudHosting(", programFileContents);
            }
            else
            {
                Assert.Contains("using Steeltoe.Extensions.Configuration;", programFileContents);
                Assert.Contains(".UseCloudFoundryHosting(", programFileContents);
            }
            Assert.Contains("using Steeltoe.Extensions.Configuration.CloudFoundry;", programFileContents);
            Assert.Contains(".AddCloudFoundry", programFileContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_v22(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_23(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_actuators_24(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                Dependencies = "Actuators",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
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
                    SteeltoeVersion = Constants.Steeltoe24,
                    Template = templateName,
                    TargetFramework = "netcoreapp3.1",
                });
            });
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public async Task CreateTemplate_empty(ITemplateService templateService, string steeltoe, string framework, string template)
        {
            Assert.NotNull(templateService);

            var files = await templateService.GenerateProjectFiles(new GeneratorModel()
            {
                ProjectName = "Foo.Bar",
                SteeltoeVersion = steeltoe,
                TargetFramework = framework,
                Template = template,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.DoesNotContain(files, file => file.Key.StartsWith("Models"));
            Assert.DoesNotContain(files, file => file.Key.EndsWith("MyCircuitBreakerCommand.cs"));
            Assert.DoesNotContain("AddCloudFoundryActuators", startUpContents);

            var projectFile = files.Find(x => x.Key == "Foo.Bar.csproj").Value;
            var targetFramework = framework.Equals("netcoreapp3.1") ? "netcoreapp3.1" : "netcoreapp2.1";
            Assert.Contains($"<TargetFramework>{targetFramework}</TargetFramework>", projectFile);
        }
    }
}
