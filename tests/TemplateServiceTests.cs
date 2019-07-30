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

using DiffMatchPatch;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Services.DotNetTemplateEngine;
using Steeltoe.InitializrTests;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.Initializr.Tests
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
            var templates = new TemplateService(_loggerFactory.CreateLogger<TemplateService>())
                .GetAvailableTemplates();
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);

            Assert.Contains(templates, x => x.ShortName == "CSharp-WebApi-2.x");
            Assert.Contains(templates, x => x.ShortName == "CSharp-WebApi-3.0");
            Assert.Contains(templates, x => x.ShortName == "CSharp-React-2.x");
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void GetDependencies(ITemplateService templateService, string templateName)
        {
            var deps = templateService.GetDependencies(templateName);
            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "OAuthConnector");
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void GetDependencies_WithFriendlyNames(ITemplateService templateService, string templateName)
        {
            var deps = templateService.GetDependencies(templateName);

            Assert.NotNull(deps);
            Assert.NotEmpty(deps);

            Assert.Contains(deps, x => x.Name == "Cloud Foundry");
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_react(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                ProjectName = "testProject",
                TemplateShortName = templateName,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.NotEmpty(startUpContents);
          }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_SqlServer(ITemplateService templateService, string templateName)
        {
            var steeltoeVersion = "2.3.0-rc1";

            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "SQLServer" },
                ProjectName = "testProject",
                TemplateShortName = templateName,
                SteeltoeVersion = steeltoeVersion,
            });

            Assert.Contains(files, file => file.Key.StartsWith("Models"));

            var fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""2.2.0"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""2.2.0"" />", fileContents);
            Assert.Contains(@"<PackageReference Include=""Steeltoe.CloudFoundry.Connector.EFCore""  Version=""" + steeltoeVersion + @""" />", fileContents);

            var program = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains(@".InitializeDbContexts()", program);

            var startup = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains(@"using Steeltoe.CloudFoundry.Connector.SqlServer.EFCore;", startup);
            Assert.Contains(@"services.AddDbContext<TestContext>(options => options.UseSqlServer(Configuration));", startup);

            if (templateName != "react")
            { // TODO: Add demo for react app
                var valuesController = files.Find(x => x.Key.EndsWith("ValuesController.cs")).Value;
                Assert.Contains(@" public ValuesController(ILogger<ValuesController> logger, [FromServices] TestContext context)", valuesController);
            }
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_DynamicLogger(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "DynamicLogger" },
                ProjectName = "testProject",
                TemplateShortName = templateName,
            });

            string fileContents = files.Find(x => x.Key == "testProject.csproj").Value;
            Assert.Contains(@"<PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=""2.2.0""/>", fileContents);

            string programFileContents = files.Find(x => x.Key == "Program.cs").Value;
            Assert.Contains(@"using Steeltoe.Extensions.Logging;", programFileContents);
            Assert.Contains(@"loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));", programFileContents);
            Assert.Contains(@"loggingBuilder.AddDynamicConsole();", programFileContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_actuators_v22(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators" },
                SteeltoeVersion = "2.2.0",
                TemplateShortName = templateName,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_actuators_23rc1(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new[] { "Actuators" },
                SteeltoeVersion = "2.3.0-RC1",
                TemplateShortName = templateName,
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.Contains("services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);", startUpContents);
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
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_empty(ITemplateService templateService, string templateName)
        {
            Assert.NotNull(templateService);

            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                TemplateShortName = templateName,
                ProjectName = "Foo.Bar",
            });

            var startUpContents = files.Find(x => x.Key == "Startup.cs").Value;

            Assert.DoesNotContain(files, file => file.Key.StartsWith("Models"));
            Assert.DoesNotContain("AddCloudFoundryActuators", startUpContents);

            var dockerFile = files.Find(x => x.Key == "Dockerfile").Value;
            Assert.NotNull(dockerFile);

            Assert.Contains("Foo.Bar.dll", dockerFile);
            Assert.Contains("Foo.Bar.csproj", dockerFile);

            var projectFile = files.Find(x => x.Key == "Foo.Bar.csproj").Value;
            Assert.Contains("<TargetFramework>netcoreapp2.2</TargetFramework>", projectFile);

            foreach (var file in files)
            {
                Assert.DoesNotContain("{{", file.Value);
                Assert.DoesNotContain("}}", file.Value);
            }
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_targetVersion21(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                Dependencies = new string[] { "Actuators,SQLServer" },
                TemplateShortName = templateName,
                ProjectName = "Foo.Bar",
                TargetFrameworkVersion = "netcoreapp2.1",
            });
            string startUpContents = files.Find(x => x.Key == "Startup.cs").Value;
            Assert.Contains("services.AddMvc();", startUpContents);

            string projectFile = files.Find(x => x.Key == "Foo.Bar.csproj").Value;
            Assert.Contains("<TargetFramework>netcoreapp2.1</TargetFramework>", projectFile);
            Assert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""2.1.1"" />", projectFile);
        }

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_targetVersion22(ITemplateService templateService, string templateName)
        {
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
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

        [Theory]
        [ClassData(typeof(AllImplementationsAndTemplates))]
        public void CreateTemplate_GeneratesCorrectVersions(ITemplateService templateService, string templateName)
        {
            var deps = templateService.GetDependencies();
            var files = templateService.GenerateProjectFiles(new Initializr.Models.GeneratorModel()
            {
                // Todo : fix this
                Dependencies = new string[] { string.Join(",", deps.Select(d => d.ShortName.ToLower()).ToArray()) },
                TemplateShortName = templateName,
                ProjectName = "Foo.Bar",
            });

            string projectFile = files.Find(x => x.Key == "Foo.Bar.csproj").Value;
            var expected = templateName != "react" ? @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
                              <PropertyGroup>
                                <TargetFramework>netcoreapp2.2</TargetFramework>
                                <AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>
                              </PropertyGroup>

                              <ItemGroup >
                                <PackageReference Include=""Microsoft.AspNetCore.App"" />
                                <PackageReference Include=""Steeltoe.Extensions.Configuration.CloudFoundryCore""  Version=""2.2.0"" />
                                <PackageReference Include=""Steeltoe.Management.ExporterCore""  Version=""2.2.0""/>
                                <PackageReference Include=""Steeltoe.Management.CloudFoundryCore"" Version=""2.2.0"" />
                                <PackageReference Include=""Steeltoe.CircuitBreaker.HystrixCore"" Version=""2.2.0"" />
                                <PackageReference Include=""MySql.Data"" Version=""8.0.16"" />
                                <PackageReference Include=""Npgsql"" Version=""4.0.4"" />
                                <PackageReference Include=""Newtonsoft.Json"" Version=""12.0.2"" />

                                <PackageReference Include=""Steeltoe.Discovery.ClientCore"" Version=""2.2.0""/>
                                <PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""2.2.0"" />
 
                                <PackageReference Include=""Microsoft.Extensions.Caching.Redis"" Version=""2.2.0"" />
                                <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""2.2.0"" />
                                <PackageReference Include=""Steeltoe.CloudFoundry.Connector.EFCore""  Version=""2.2.0"" />
                                <PackageReference Include=""Steeltoe.CloudFoundry.ConnectorCore""  Version=""2.2.0"" />

                                <PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL""  Version=""2.2.0"" />
                                <PackageReference Include=""MongoDB.Driver"" Version=""2.8.1"" />

                                <PackageReference Include=""RabbitMQ.Client""  Version=""5.1.0"" />
                                <PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=""2.2.0""/>
                             </ItemGroup>
                              <ItemGroup Condition=""'$(BUILD)' == ''"">
                                <PackageReference Include=""Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore"" Version=""2.2.0"" />
                                <PackageReference Include=""RabbitMQ.Client"" Version=""5.1.0"" />
                              </ItemGroup>

                              <ItemGroup Condition=""'$(BUILD)' == 'LOCAL'"">
                                <PackageReference Include=""Steeltoe.CircuitBreaker.Hystrix.MetricsEventsCore"" Version=""2.2.0"" />
                                <PackageReference Include=""System.Threading.ThreadPool"" Version=""4.3.0"" />
                              </ItemGroup>
</Project>
" :
@"
<Project Sdk=""Microsoft.NET.Sdk.Web"">

    <PropertyGroup>
        <TargetFramework>netcoreapp2.2</TargetFramework>
        <TypeScriptCompileBlocked>true</TypeScriptCompileBlocked>
        <TypeScriptToolsVersion>Latest</TypeScriptToolsVersion>
        <IsPackable>false</IsPackable>
        <SpaRoot>ClientApp\</SpaRoot>
        <DefaultItemExcludes>$(DefaultItemExcludes);$(SpaRoot)node_modules\**</DefaultItemExcludes>
    </PropertyGroup>

  <ItemGroup>
    <!-- Don't publish the SPA source files, but do show them in the project files list -->
    <Content Remove=""$(SpaRoot)**"" />
    <None Remove=""$(SpaRoot)**"" />
    <None Include=""$(SpaRoot)**"" Exclude=""$(SpaRoot)node_modules\**"" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.App"" />
    <PackageReference Include=""Microsoft.AspNetCore.Razor.Design"" Version=""2.2.0"" PrivateAssets=""All"" />
    <PackageReference Include=""Steeltoe.Extensions.Configuration.CloudFoundryCore""  Version=""2.2.0"" />
    <PackageReference Include=""Steeltoe.Management.ExporterCore""  Version=""2.2.0""/>               

    <PackageReference Include=""Steeltoe.Management.CloudFoundryCore"" Version=""2.2.0"" />
    <PackageReference Include=""Steeltoe.CircuitBreaker.HystrixCore"" Version=""2.2.0"" />
    <PackageReference Include=""MySql.Data"" Version=""8.0.16"" />
    <PackageReference Include=""Npgsql"" Version=""4.0.4"" />
    <PackageReference Include=""Newtonsoft.Json"" Version=""12.0.2"" />
    <PackageReference Include=""Steeltoe.Discovery.ClientCore"" Version=""2.2.0""/>
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""2.2.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""2.2.0"" />
    <PackageReference Include=""Microsoft.Extensions.Caching.Redis"" Version=""2.2.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""2.2.0"" />
    <PackageReference Include=""Steeltoe.CloudFoundry.Connector.EFCore"" Version=""2.2.0"" />
    <PackageReference Include=""Steeltoe.CloudFoundry.ConnectorCore""  Version=""2.2.0"" />

    <PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL""  Version=""2.2.0"" />
    <PackageReference Include=""MongoDB.Driver"" Version=""2.8.1"" />
    <PackageReference Include=""RabbitMQ.Client""  Version=""5.1.0"" />
    <PackageReference Include=""Steeltoe.Extensions.Logging.DynamicLogger"" Version=""2.2.0""/>
    </ItemGroup>
   <ItemGroup Condition=""'$(BUILD)' == ''"">
    <PackageReference Include=""Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore"" Version=""2.2.0"" />
    <PackageReference Include=""RabbitMQ.Client"" Version=""5.1.0"" />
   </ItemGroup>
   <ItemGroup Condition=""'$(BUILD)' == 'LOCAL'"">
    <PackageReference Include=""Steeltoe.CircuitBreaker.Hystrix.MetricsEventsCore"" Version=""2.2.0"" />
    <PackageReference Include=""System.Threading.ThreadPool"" Version=""4.3.0"" />
   </ItemGroup>
  <Target Name=""DebugEnsureNodeEnv"" BeforeTargets=""Build"" Condition="" '$(Configuration)' == 'Debug' And !Exists('$(SpaRoot)node_modules') "">
    <!-- Ensure Node.js is installed -->
    <Exec Command=""node --version"" ContinueOnError=""true"">
      <Output TaskParameter=""ExitCode"" PropertyName=""ErrorCode"" />
    </Exec>
    <Error Condition=""'$(ErrorCode)' != '0'"" Text=""Node.js is required to build and run this project. To continue, please install Node.js from https://nodejs.org/, and then restart your command prompt or IDE."" />
    <Message Importance=""high"" Text=""Restoring dependencies using 'npm'. This may take several minutes..."" />
    <Exec WorkingDirectory=""$(SpaRoot)"" Command=""npm install"" />
  </Target>

  <Target Name=""PublishRunWebpack"" AfterTargets=""ComputeFilesToPublish"">
    <!-- As part of publishing, ensure the JS resources are freshly built in production mode -->
    <Exec WorkingDirectory=""$(SpaRoot)"" Command=""npm install"" />
    <Exec WorkingDirectory=""$(SpaRoot)"" Command=""npm run build"" />

    <!-- Include the newly-built files in the publish output -->
    <ItemGroup>
      <DistFiles Include=""$(SpaRoot)build\**"" />
      <ResolvedFileToPublish Include=""@(DistFiles->'%(FullPath)')"" Exclude=""@(ResolvedFileToPublish)"">
        <RelativePath>%(DistFiles.Identity)</RelativePath>
        <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
      </ResolvedFileToPublish>
    </ItemGroup>
  </Target>

</Project>


";

            Assert.True(DiffIgnoreWhitespace(expected, projectFile, out string diffMessage), diffMessage);
        }

        private bool DiffIgnoreWhitespace(string a, string b, out string diffMessage)
        {
            var dmp = DiffMatchPatchModule.Default;
            var a_min = a.Replace(" ", string.Empty).Replace("    ", string.Empty);
            var b_min = b.Replace(" ", string.Empty).Replace("    ", string.Empty);
            var diffs = dmp.DiffMain(b_min, a_min, true);

            var filtered_diffs = diffs.Where(x => x.Operation != Operation.Equal && x.Text.Any(c => !char.IsWhiteSpace(c))).Take(3).ToList();

            var diffStrings = string.Join("\r\n", filtered_diffs.Select(d => (d.Operation == Operation.Insert ? '+' : '-') + $" {d.Text} "));
            diffMessage = diffStrings + "in " + b;
            return !filtered_diffs.Any();
        }
    }
}
