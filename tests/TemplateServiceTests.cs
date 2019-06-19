using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Steeltoe.Initializr.Models;
using Steeltoe.Initializr.Services;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.InitializrTests
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

            var templates = templateService.GetAvailableTemplates();
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);

            Assert.Contains(templates, x => x.ShortName == "steeltoe");
            Assert.Contains(templates, x => x.ShortName == "steeltoe2");
        }

        [Fact]
        public void GetDependencies ()
        {
            var templateService = new TemplateService();

            var deps = templateService.GetDependencies();
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "OAuthConnector");

            deps = templateService.GetDependencies("steeltoe");
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.DoesNotContain(deps, x => x.Name == "OAuthConnector");
        }
        [Fact]
        public void GetDependencies_WithFriendlyNames()
        {
            var settings = new Dictionary<string, string>()
            {
                ["FriendlyNames:CloudFoundry"] = "Cloud Foundry",

            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            

             //   .Add()
            var templateService = new TemplateService(configuration);

            var deps = templateService.GetDependencies();
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "Cloud Foundry");

        }
        [Fact]
        public void CreateTemplate_actuators()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[]{ "Actuators"}).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);

            Assert.Contains("using Steeltoe.Management.Hypermedia;", startUpContents);
            Assert.Contains("using Steeltoe.Management.Endpoint;", startUpContents);
            Assert.Contains("using Steeltoe.Management.CloudFoundry;", startUpContents);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);

        }
        [Fact]
        public void CreateTemplate_discovery()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Discovery" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);

            Assert.Contains("using Steeltoe.Discovery.Client;", startUpContents);
            Assert.Contains("services.AddDiscoveryClient(Configuration);", startUpContents);
            Assert.Contains("app.UseDiscoveryClient();", startUpContents);

        }
        [Fact]
        public void CreateTemplate_actuators_circuitbreakers()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Actuators", "CircuitBreaker" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            Assert.Contains("using Steeltoe.CircuitBreaker.Hystrix;", startUpContents);


        }
        [Fact]
        public void CreateTemplate_MySql()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "MySql" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);

            Assert.Contains(".AddMySqlConnection(", startUpContents);
        }
        [Fact]
        public void CreateTemplate_MySql_EFCore()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "MySqlEFCore" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql;", startUpContents);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MySql.EFCore;", startUpContents);

        }
        [Fact]
        public void CreateTemplate_postgresql()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Postgres" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql;", startUpContents);

            Assert.Contains("services.AddPostgresConnection(Configuration);", startUpContents);

        }
        [Fact]
        public void CreateTemplate_postgresEFCore()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "PostgresEFCore" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;", startUpContents);

            Assert.Contains("services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));", startUpContents);

        }

        [Fact]
        public void CreateTemplate_RabbitMQ()
        {
            var templateService = new TemplateService();
            
            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "RabbitMQ" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.RabbitMQ;", startUpContents);

            Assert.Contains("services.AddRabbitMQConnection(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_Redis()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Redis" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.Redis", startUpContents);
            Assert.Contains("services.AddDistributedRedisCache(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_MongoDB()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "MongoDB" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.MongoDb;", startUpContents);
            Assert.Contains("services.AddMongoClient(Configuration);", startUpContents);
        }

        [Fact]
        public void CreateTemplate_OauthConnector()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "OAuthConnector" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("using Steeltoe.CloudFoundry.Connector.OAuth;", startUpContents);
            Assert.Contains("services.AddOAuthServiceOptions(Configuration);", startUpContents);
        }
        [Fact]
        public void CreateTemplate_SqlServer()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "SQLServer" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var filePath = Path.Combine(outFolder, "testProject.csproj");
            Assert.True(File.Exists(filePath));
            string fileContents = File.ReadAllText(filePath);
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""$(AspNetCoreVersion)"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""$(AspNetCoreVersion)"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Steeltoe.CloudFoundry.Connector.EFCore"" Version=""$(SteeltoeConnectorVersion)"" />", fileContents);
        }

        [Fact]
        public void CreateTemplate_DynamicLogger()
        {
            var templateService = new TemplateService();

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "DynamicLogger" }).Result;
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var filePath = Path.Combine(outFolder, "testProject.csproj");
            Assert.True(File.Exists(filePath));
            string fileContents = File.ReadAllText(filePath);
            Assert.Contains(@"<PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=""$(SteeltoeLoggingVersion)""/>", fileContents);

            var programFilePath = Path.Combine(outFolder, "Program.cs");
            Assert.True(File.Exists(programFilePath));
            string programFileContents = File.ReadAllText(programFilePath);
            Assert.Contains(@"using Steeltoe.Extensions.Logging;", programFileContents);
            Assert.Contains(@"loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));", programFileContents);
            Assert.Contains(@"loggingBuilder.AddDynamicConsole();", programFileContents);
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

            var outFolder = templateService.GenerateProject("steeltoe2", "testProject", new[] { "Actuators", "SteeltoeVersion=2.1.0" }).Result;
            Console.WriteLine("outFolder " + outFolder);
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.DoesNotContain("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration);", startUpContents);

        }

        [Fact]
        public void CreateTemplate_actuators_v3()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

            var outFolder = templateService.GenerateProject("steeltoe", "testProject", new[] { "Actuators"}).Result;
            Console.WriteLine("outFolder " + outFolder);
            Assert.NotNull(outFolder);
            Assert.True(Directory.Exists(outFolder));
            var startupPath = Path.Combine(outFolder, "Startup.cs");
            Assert.True(File.Exists(startupPath));
            string startUpContents = File.ReadAllText(startupPath);
            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
            Assert.DoesNotContain("services.AddCloudFoundryActuators(Configuration);", startUpContents);

        }
        [Fact]
        public void CreateTemplate_empty()
        {
            var templateService = new TemplateService();

            Assert.NotNull(templateService);

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
