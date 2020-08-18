using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
{{#MongoDB}}
using Steeltoe.Connector.MongoDb;
{{/MongoDB}}
{{#MySqlOrMySqlEFCore}}
using Steeltoe.Connector.MySql;
{{/MySqlOrMySqlEFCore}}
{{#MySqlEFCore}}
using Steeltoe.Connector.MySql.EFCore;
{{/MySqlEFCore}}
{{#OAuthConnector}}
using Steeltoe.Connector.OAuth;
{{/OAuthConnector}}
{{#Postgres}}
using Steeltoe.Connector.PostgreSql;
{{/Postgres}}
{{#PostgresEFCore}}
using Steeltoe.Connector.PostgreSql.EFCore;
{{/PostgresEFCore}}
{{#RabbitMQ}}
using Steeltoe.Connector.RabbitMQ;
{{/RabbitMQ}}
{{#Redis}}
using Steeltoe.Connector.Redis;
{{/Redis}}
{{#SQLServer}}
using Steeltoe.Connector.SqlServer.EFCore;
{{/SQLServer}}
{{#Discovery}}
using Steeltoe.Discovery.Client;
{{/Discovery}}
{{#Actuators}}
using Steeltoe.Management.CloudFoundry;
{{/Actuators}}
{{#RequiresHttps}}
using Microsoft.AspNetCore.HttpsPolicy;
{{/RequiresHttps}}
{{#Auth}}
using Microsoft.AspNetCore.Authentication;
{{/Auth}}
{{#OrganizationalAuth}}
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
{{/OrganizationalAuth}}
{{#IndividualB2CAuth}}
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
{{/IndividualB2CAuth}}
{{#CircuitBreaker}}
using Steeltoe.CircuitBreaker.Hystrix;
{{/CircuitBreaker}}

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
            services.AddCloudFoundryActuators(Configuration);
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

{{#Discovery}}
            app.UseDiscoveryClient();
{{/Discovery}}
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
