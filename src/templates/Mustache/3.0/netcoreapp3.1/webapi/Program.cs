using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
{{#AzureSpringCloud}}
using Microsoft.Azure.SpringCloud.Client;
{{/AzureSpringCloud}}
{{#DynamicSerilog}}
using Steeltoe.Extensions.Logging.DynamicSerilog;
{{/DynamicSerilog}}
{{#DynamicLogger}}
    using Steeltoe.Extensions.Logging;
{{/DynamicLogger}}
{{#CloudFoundry}}
using Steeltoe.Common.Hosting;
    {{^ConfigServer}}
using Steeltoe.Extensions.Configuration.CloudFoundry;
    {{/ConfigServer}}
    {{#Actuators}}
using Steeltoe.Management.CloudFoundry;
    {{/Actuators}}
{{/CloudFoundry}}
{{^CloudFoundry}}
    {{#Actuators}}
    using Steeltoe.Management.Endpoint;
    {{/Actuators}}
{{/CloudFoundry}}
{{^Actuators}}
    {{#DynamicLogger}}
    using Steeltoe.Management.Endpoint;
    {{/DynamicLogger}}
{{/Actuators}}
{{#ConfigServer}}
using Steeltoe.Extensions.Configuration.ConfigServer;
{{/ConfigServer}}
{{#PlaceholderConfig}}
using Steeltoe.Extensions.Configuration.Placeholder;
{{/PlaceholderConfig}}
{{#RandomValueConfig}}
using Steeltoe.Extensions.Configuration.RandomValue;
{{/ RandomValueConfig}}

namespace {{ProjectNameSpace}}
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
            .Build()
            {{#AnyEFCore}}
            .InitializeDbContexts()
            {{/AnyEFCore}}
            .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider(configure => configure.ValidateScopes = false)
{{#CloudFoundry}}
                .UseCloudHosting() //Enable listening on a Env provided port
                {{#Actuators}}
                .AddCloudFoundryActuators()
                {{/Actuators}}
{{^ConfigServer}}
                .AddCloudFoundryConfiguration() //Add cloudfoundry environment variables as a configuration source
{{/ConfigServer}}
{{/CloudFoundry}}
{{^CloudFoundry}}
    {{#Actuators}}
                .AddAllActuators()
    {{/Actuators}}
{{/CloudFoundry}}
{{#ConfigServer}}
                .AddConfigServer()
{{/ConfigServer}}
{{#PlaceholderConfig}}
                .AddPlaceholderResolver()
{{/PlaceholderConfig}}
{{#RandomValueConfig}}
                .ConfigureAppConfiguration((b) => b.AddRandomValueSource())
{{/RandomValueConfig}}
{{#AzureSpringCloud}}
                .UseAzureSpringCloudService()
{{/AzureSpringCloud}}

{{#DynamicSerilog}}
                .ConfigureLogging((context, loggingBuilder) => loggingBuilder.AddSerilogDynamicConsole())
{{/DynamicSerilog}}
{{#DynamicLogger}}
        {{^ActuatorsOrCloudFoundry}}
        .AddLoggersActuator()
        .ConfigureLogging((context, loggingBuilder) => loggingBuilder.AddDynamicConsole())
        {{/ActuatorsOrCloudFoundry}}
{{/DynamicLogger}}
                .UseStartup<Startup>();
            return builder;
        }
    }
}
