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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.InitializrTests
{
    public class MustacheTemplateServiceTests : XunitLoggingBase
    {
        private ILogger<MustacheTemplateService> _logger;

        public MustacheTemplateServiceTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            _logger = loggerFactory.CreateLogger<MustacheTemplateService>();
        }

        [Fact]
        public void GetAvailableTemplates_returnsTemplates()
        {
            var templateService = new MustacheTemplateService(_logger);

            var templates = templateService.GetAvailableTemplates();
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);

        }

        //[Fact]
        //public void GetDependencies()
        //{
        //    var templateService = new SteeltoeTemplateService(null);

        //    var deps = templateService.GetDependencies();
        //    Assert.NotNull(deps);
        //    Assert.NotEmpty(deps);

        //    Assert.Contains(deps, x => x.Name == "OAuthConnector");

        //    deps = templateService.GetDependencies("steeltoe");
        //    Assert.NotNull(deps);
        //    Assert.NotEmpty(deps);

        //    Assert.DoesNotContain(deps, x => x.Name == "OAuthConnector");
        //}

        //[Fact]
        //public void GetDependencies_WithFriendlyNames()
        //{
        //    var settings = new Dictionary<string, string>()
        //    {
        //        ["FriendlyNames:CloudFoundry"] = "Cloud Foundry",
        //    };
        //    var configuration = new ConfigurationBuilder()
        //        .AddInMemoryCollection(settings)
        //        .Build();

        //    var templateService = new TemplateService(configuration, null);

        //    var deps = templateService.GetDependencies();
        //    Assert.NotNull(deps);
        //    Assert.NotEmpty(deps);

        //    Assert.Contains(deps, x => x.Name == "Cloud Foundry");
        //}

        [Fact]
        public void CreateTemplate_actuators()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
                {
                    Dependencies = new[] { "Actuators" },
                });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Management.Hypermedia;", startUpContents);
            Assert.Contains("using Steeltoe.Management.Endpoint;", startUpContents);
            Assert.Contains("using Steeltoe.Management.CloudFoundry;", startUpContents);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_discovery()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Discovery" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.Discovery.Client;", startUpContents);
            Assert.Contains("services.AddDiscoveryClient(Configuration);", startUpContents);
            Assert.Contains("app.UseDiscoveryClient();", startUpContents);
        }

        [Fact]
        public void CreateTemplate_actuators_circuitbreakers()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators,CircuitBreaker" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            Assert.Contains("using Steeltoe.CircuitBreaker.Hystrix;", startUpContents);
        }

        [Fact]
        public void CreateTemplate_MySql()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MySql" },
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);

            Assert.Contains(".AddMySqlConnection(", startUpContents);
        }

        [Fact]
        public void CreateTemplate_MySql_EFCore()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MySqlEFCore" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql.EFCore;", startUpContents);
        }

        [Fact]
        public void CreateTemplate_postgresql()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Postgres" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql;", startUpContents);

            Assert.Contains("services.AddPostgresConnection(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_postgresEFCore()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "PostgresEFCore" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;", startUpContents);

            Assert.Contains("services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));", startUpContents);
        }

        [Fact]
        public void CreateTemplate_RabbitMQ()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "RabbitMQ" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.RabbitMQ;", startUpContents);

            Assert.Contains("services.AddRabbitMQConnection(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_Redis()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Redis" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.Redis", startUpContents);
            Assert.Contains("services.AddDistributedRedisCache(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_MongoDB()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "MongoDB" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MongoDb;", startUpContents);
            Assert.Contains("services.AddMongoClient(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_OauthConnector()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "OAuthConnector" },
            });

            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.OAuth;", startUpContents);
            Assert.Contains("services.AddOAuthServiceOptions(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_SqlServer()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "SQLServer" },
                ProjectName = "testProject",
            });
            string fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""$(AspNetCoreVersion)"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""$(AspNetCoreVersion)"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Steeltoe.CloudFoundry.Connector.EFCore"" Version=""$(SteeltoeConnectorVersion)"" />", fileContents);
        }

        [Fact]
        public void CreateTemplate_DynamicLogger()
        {
            var templateService = new MustacheTemplateService(_logger);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
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

        [Fact]
        public void CreateTemplate_actuators_cloudFoundry()
        {
            var templateService = new MustacheTemplateService(_logger);

            Assert.NotNull(templateService);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel()
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
        //}

        //[Fact]
        //public void CreateTemplate_actuators_v3()
        //{
        //    var templateService = new MustacheTemplateService(_logger);

        //    Assert.NotNull(templateService);

        //    var outFolder = templateService.GenerateProject("steeltoe", "testProject", new[] { "Actuators" }).Result;
        //    Console.WriteLine("outFolder " + outFolder);
        //    Assert.NotNull(outFolder);
        //    Assert.True(Directory.Exists(outFolder));
        //    var startupPath = Path.Combine(outFolder, "Startup.cs");
        //    Assert.True(File.Exists(startupPath));
        //    string startUpContents = File.ReadAllText(startupPath);
        //    Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
        //    Assert.DoesNotContain("services.AddCloudFoundryActuators(Configuration);", startUpContents);
        //}

        [Fact]
        public void CreateTemplate_empty()
        {
            var templateService = new MustacheTemplateService(_logger);

            Assert.NotNull(templateService);

            var files = templateService.GenerateProject(new Initializr.Models.GeneratorModel());
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.DoesNotContain("AddCloudFoundryActuators", startUpContents);
        }
    }
}
