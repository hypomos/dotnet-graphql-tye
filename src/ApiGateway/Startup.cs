using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;

namespace ApiGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            AddHttpClientForStitching(services, Constants.MetaDataApi);
            AddHttpClientForStitching(services, Constants.StorageApi);
            AddHttpClientForStitching(services, Constants.UserApi);

            var redisUri = Configuration.GetConnectionString(Constants.RedisServiceName)!;
            services.AddSingleton(ConnectionMultiplexer.Connect(redisUri));

            services.AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                .AddRemoteSchemasFromRedis(Constants.RedisConfigurationName, sp => sp.GetRequiredService<ConnectionMultiplexer>());
        }

        private void AddHttpClientForStitching(IServiceCollection services, string name)
        {
            var uri = Configuration.GetConnectionString(name)!;
            services.AddHttpClient(name, (sp, client) => { client.BaseAddress = new Uri(uri); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks(Constants.HealthPath);
                endpoints.MapGraphQL();
            });
        }
    }
}