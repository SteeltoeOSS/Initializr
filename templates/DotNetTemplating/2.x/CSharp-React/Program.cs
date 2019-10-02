using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#if (Actuators || DynamicLogger)
using Steeltoe.Extensions.Logging;
#endif
#if (CloudFoundry)
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
#endif
#if (ConfigServer)
using Steeltoe.Extensions.Configuration.ConfigServer;
#endif
#if(PlaceholderConfig)
using Steeltoe.Extensions.Configuration.PlaceholderCore;
#endif
#if(RandomValueConfig)
using Steeltoe.Extensions.Configuration.RandomValue;
#endif
namespace Company.WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .Build()
#if (AnyEFCore)
                .InitializeDbContexts()
#endif
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider(configure => configure.ValidateScopes = false)
#if (CloudFoundry)
                .UseCloudFoundryHosting() //Enable listening on a Env provided port
                .AddCloudFoundry() //Add cloudfoundry environment variables as a configuration source
#endif
#if (ConfigServer)
                .AddConfigServer()
#endif
#if (PlaceholderConfig)
                .AddPlaceholderResolver()
#endif
#if (RandomValueConfig)
                .ConfigureAppConfiguration((b) => b.AddRandomValueSource())
#endif
                .UseStartup<Startup>();
#if (Actuators || DynamicLogger)
            builder.ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                loggingBuilder.AddDynamicConsole();
            });
#endif
            return builder;
        }
    }
}
