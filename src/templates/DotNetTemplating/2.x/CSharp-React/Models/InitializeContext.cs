using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.WebApplication1
{
    public static class InitializeContext
    {
        public static IWebHost InitializeDbContexts(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    SampleData.InitializeMyContexts(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<WebHostBuilder>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
            return host;
        }
    }
}
