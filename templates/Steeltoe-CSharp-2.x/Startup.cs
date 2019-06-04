using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if (RequiresHttps)
using Microsoft.AspNetCore.HttpsPolicy;
#endif
using Microsoft.AspNetCore.Mvc;
#if (OrganizationalAuth || IndividualB2CAuth)
using Microsoft.AspNetCore.Authentication;
#endif
#if (OrganizationalAuth)
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
#endif
#if (IndividualB2CAuth)
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#if (MSSql)
using Steeltoe.CloudFoundry.Connector.MySql;
#endif
#if(Actuators || CloudFoundry)
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint;
    #if(SteeltoeVersion == "2.2.0")
using Steeltoe.Management.Hypermedia;
    #endif
#endif
#if(Hystrix)
using Steeltoe.CircuitBreaker.Hystrix;
#endif
#if(MySql)
using Steeltoe.CloudFoundry.Connector.MySql;
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
#if (OrganizationalAuth)
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
#elif (IndividualB2CAuth)
            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));
#endif
#if (MySql)
            services.AddMySqlConnection(Configuration);
#endif
#if(SteeltoeVersion == "2.2.0")
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
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
#if (RequiresHttps)
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
#endif
#if (OrganizationalAuth || IndividualAuth)
            app.UseAuthentication();
#endif

#if (SteeltoeVersion == "2.2.0")
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
