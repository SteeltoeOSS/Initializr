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


namespace Company.WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider(configure => configure.ValidateScopes = false)
#if (CloudFoundry)
                .UseCloudFoundryHosting(5555) //Enable listening on a Env provided port
                .AddCloudFoundry() //Add cloudfoundry environment variables as a configuration source
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
