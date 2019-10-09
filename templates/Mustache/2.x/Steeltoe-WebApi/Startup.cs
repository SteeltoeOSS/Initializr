using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
{{#RequiresHttps}}
using Microsoft.AspNetCore.HttpsPolicy;
{{/RequiresHttps}}
using Microsoft.AspNetCore.Mvc;
{{#Auth}}
using Microsoft.AspNetCore.Authentication;
{{/Auth}}
{{#OrganizationalAuth}}
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
{{/OrganizationalAuth}}
{{#IndividualB2CAuth}}
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
{{/IndividualB2CAuth}}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
{{#ActuatorsOrCloudFoundry}}
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Hypermedia;
{{/ActuatorsOrCloudFoundry}}
{{#CloudFoundry}}
using Steeltoe.Extensions.Configuration.CloudFoundry; 
{{/CloudFoundry}}
{{#CircuitBreaker}}
using Steeltoe.CircuitBreaker.Hystrix;
{{/CircuitBreaker}}
{{#MySqlOrMySqlEFCore}}
using Steeltoe.CloudFoundry.Connector.MySql;
{{/MySqlOrMySqlEFCore}}
{{#MySqlEFCore}}
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
{{/MySqlEFCore}}
{{#SQLServer}}
using Steeltoe.CloudFoundry.Connector.SqlServer;
{{/SQLServer}}
{{#Discovery}}
using Steeltoe.Discovery.Client;
{{/Discovery}}
{{#Postgres}}
using Steeltoe.CloudFoundry.Connector.PostgreSql;
{{/Postgres}}
{{#RabbitMQ}}
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
{{/RabbitMQ}}
{{#Redis}}
using Steeltoe.CloudFoundry.Connector.Redis;
{{/Redis}}
{{#MongoDB}}
using Steeltoe.CloudFoundry.Connector.MongoDb;
{{/MongoDB}}
{{#OAuthConnector}}
using Steeltoe.CloudFoundry.Connector.OAuth;
{{/OAuthConnector}}
{{#PostgresEFCore}}
using Steeltoe.CloudFoundry.Connector.PostgreSql.EFCore;
{{/PostgresEFCore}}

namespace {{ProjectNameSpace}}
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
{{#OrganizationalAuth}}
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
{{/OrganizationalAuth}}
{{#IndividualB2CAuth}}
            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));
{{/IndividualB2CAuth}}
{{#MySql}}
            services.AddMySqlConnection(Configuration);
{{/MySql}}
{{#Actuators}}
{{#CloudFoundry}}
            services.ConfigureCloudFoundryOptions(Configuration);
            services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
{{/CloudFoundry}}
{{^CloudFoundry}}
            services.AddCloudFoundryActuators(Configuration);
{{/CloudFoundry}}
{{/Actuators}}
{{#Discovery}}
            services.AddDiscoveryClient(Configuration);
{{/ Discovery}}
{{#Postgres}}
            services.AddPostgresConnection(Configuration);
{{/Postgres}}
{{#RabbitMQ}}
            services.AddRabbitMQConnection(Configuration);
{{/RabbitMQ}}
{{#Redis}}
            // Add the Redis distributed cache.

            // We are using the Steeltoe Redis Connector to pickup the CloudFoundry
            // Redis Service binding and use it to configure the underlying RedisCache
            // This adds a IDistributedCache to the container
            services.AddDistributedRedisCache(Configuration);

            // This works like the above, but adds a IConnectionMultiplexer to the container
            // services.AddRedisConnectionMultiplexer(Configuration);
{{/Redis}}
{{#MongoDB}}
            services.AddMongoClient(Configuration);
{{/MongoDB}}

{{#OAuthConnector}}
            services.AddOAuthServiceOptions(Configuration);
{{/OAuthConnector}}
{{#PostgresEFCore}}
            // Add Context and use Postgres as provider ... provider will be configured from VCAP_ info
            // services.AddDbContext<MyDbContext>(options => options.UseNpgsql(Configuration));
{{/PostgresEFCore}}
{{#SQLServer}}
            services.AddSqlServerConnection(Configuration);
{{/SQLServer}}
{{#TargetFrameworkVersion22}}
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
{{/TargetFrameworkVersion22}}
{{^TargetFrameworkVersion22}}
            services.AddMvc();
{{/TargetFrameworkVersion22}}
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            {{#RequiresHttps}}
            else
            {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            {{/RequiresHttps}}
            {{#Auth}}
            app.UseAuthentication();
            {{/Auth}}

            {{#Actuators}}
            {{#CloudFoundry}}
            app.UseCloudFoundryActuators(MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
            {{/CloudFoundry}}
            {{^CloudFoundry}}
            app.UseCloudFoundryActuators();
            {{/CloudFoundry}}
            {{/Actuators}}
           
            {{#Discovery}}
            app.UseDiscoveryClient();
            {{/Discovery}}
            app.UseMvc();
        }
    }
}
