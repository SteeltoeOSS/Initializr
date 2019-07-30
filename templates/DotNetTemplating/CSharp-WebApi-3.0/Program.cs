using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
            CreateHostBuilder(args)
                .Build()
#if (AnyEFCore)
                .InitializeDbContexts()
#endif
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
#if (CloudFoundry)
                    webBuilder.UseCloudFoundryHosting(5555); //Enable listening on a Env provided port
                    webBuilder.AddCloudFoundry(); //Add cloudfoundry environment variables as a configuration source
#endif
                    webBuilder.UseStartup<Startup>();
#if (Actuators || DynamicLogger)
                    webBuilder.ConfigureLogging((hostingContext, loggingBuilder) =>
                    {
                        loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        loggingBuilder.AddDynamicConsole();
                    });
#endif
                });
    }
}
