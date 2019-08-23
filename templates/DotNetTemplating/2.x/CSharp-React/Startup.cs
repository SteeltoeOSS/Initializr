using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if (!NoHttps)
using Microsoft.AspNetCore.HttpsPolicy;
#endif
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#if (Actuators || CloudFoundry)
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint;
#if (SteeltoeVersion == "2.2.0" || SteeltoeVersion == "2.3.0")
using Steeltoe.Management.Hypermedia;
#endif
#endif
#if (CircuitBreaker)
using Steeltoe.CircuitBreaker.Hystrix;
#endif
#if (MySql || MySqlEFCore)
using Steeltoe.CloudFoundry.Connector.MySql;
#endif
#if(MySqlEFCore)
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
#endif
#if (Discovery)
using Steeltoe.Discovery.Client;
#endif
#if (Postgres)
using Steeltoe.CloudFoundry.Connector.PostgreSql;
#endif
#if (RabbitMQ)
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
#endif
#if (Redis)
using Steeltoe.CloudFoundry.Connector.Redis;
#endif
#if (MongoDB)
using Steeltoe.CloudFoundry.Connector.MongoDb;
#endif
#if (OAuthConnector)
using Steeltoe.CloudFoundry.Connector.OAuth;
#endif
#if (PostgresEFCore)
using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;
#endif
#if (SQLServerEFCore)
using Steeltoe.CloudFoundry.Connector.SqlServer.EFCore;
#endif
namespace Company.WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if (MySql)
            services.AddMySqlConnection(Configuration);
#endif
#if (SteeltoeVersion == "2.2.0" || SteeltoeVersion == "2.3.0")
#if (Actuators && CloudFoundry)
	    services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
#elif (Actuators)
	    services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.Actuator);
#endif
#else
#if (Actuators && CloudFoundry)
	    services.AddCloudFoundryActuators(Configuration);
#elif (Actuators)
	    services.AddCloudFoundryActuators(Configuration);
#endif

#endif
#if (Discovery)
            services.AddDiscoveryClient(Configuration);
#endif
#if (Postgres)
            services.AddPostgresConnection(Configuration);
#endif
#if (RabbitMQ)
            services.AddRabbitMQConnection(Configuration);
#endif
#if (Redis)
            // Add the Redis distributed cache.

            // We are using the Steeltoe Redis Connector to pickup the CloudFoundry
            // Redis Service binding and use it to configure the underlying RedisCache
            // This adds a IDistributedCache to the container
            services.AddDistributedRedisCache(Configuration);

            // This works like the above, but adds a IConnectionMultiplexer to the container
            services.AddRedisConnectionMultiplexer(Configuration);
#endif
#if (MongoDB)
             services.AddMongoClient(Configuration);
#endif
#if (OAuthConnector)
             services.AddOAuthServiceOptions(Configuration);
#endif
#if (PostgresEFCore)
              // Add Context and use Postgres as provider ... provider will be configured from VCAP_ info
              // services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));
#endif
#if (SQLServerEFCore)
               services.AddDbContext<TestContext>(options => options.UseSqlServer(Configuration));
#endif

#if (FrameworkVersion == "netcoreapp2.2")
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
#else
            services.AddMvc();
#endif

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
#if (!NoHttps)
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
#else
            }

#endif

#if (SteeltoeVersion == "2.2.0" || SteeltoeVersion == "2.3.0")
#if (Actuators && CloudFoundry)
            app.UseCloudFoundryActuators(MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
#elif (Actuators)
	    app.UseCloudFoundryActuators(MediaTypeVersion.V2, ActuatorContext.Actuator);
#endif
#else
#if (Actuators && CloudFoundry)
            app.UseCloudFoundryActuators();
#elif (Actuators)
	    app.UseCloudFoundryActuators();
#endif

#endif
#if (Discovery)
           app.UseDiscoveryClient();
#endif

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
