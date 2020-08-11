using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
{{#AzureSpringCloud}}
using Microsoft.Azure.SpringCloud.Client;
{{/AzureSpringCloud}}
{{#ActuatorsOrDynamicLogger}}
using Steeltoe.Extensions.Logging.DynamicSerilog;
{{/ActuatorsOrDynamicLogger}}
{{#CloudFoundry}}
{{^ConfigServer}}
using Steeltoe.Common.Hosting;
using Steeltoe.Extensions.Configuration.CloudFoundry;
{{/ConfigServer}}
{{/CloudFoundry}}
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
{{^ConfigServer}}
                .AddCloudFoundryConfiguration() //Add cloudfoundry environment variables as a configuration source
{{/ConfigServer}}
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
{{#ActuatorsOrDynamicLogger}}
                .ConfigureLogging((context, builder) => builder.AddSerilogDynamicConsole())
{{/ActuatorsOrDynamicLogger}}
                .UseStartup<Startup>();
            return builder;
        }
    }
}
