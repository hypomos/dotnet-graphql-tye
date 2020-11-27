using Contract;
using HotChocolate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace MetaData
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            var redisUri = Configuration.GetConnectionString(Constants.RedisServiceName)!;

            services
                .AddSingleton(ConnectionMultiplexer.Connect(redisUri))
                .AddSingleton<MetaDataRepository>()
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .InitializeOnStartup()
                .PublishSchemaDefinition(c => c
                    .SetName(Constants.MetaDataApi)
                    .IgnoreRootTypes()
                    .AddTypeExtensionsFromFile("./Stitching.graphql")
                    .PublishToRedis(Constants.RedisConfigurationName, sp => sp.GetRequiredService<ConnectionMultiplexer>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks(Constants.HealthPath);
                endpoints.MapGraphQL();
            });
        }
    }

    public class MetaDataRepository
    {
        private readonly Dictionary<int, MetaData> _metaDatas;

        public MetaDataRepository()
        {
            _metaDatas = new MetaData[]
            {
                new MetaData(1, "MyImage.jpg", 3.5f, 2)
            }.ToDictionary(t => t.Id);
        }

        public MetaData GetMetaData(int id) => _metaDatas[id];

        public IEnumerable<MetaData> GetMetaDataByStorageId(int storageId) => 
            _metaDatas.Values
                .OrderBy(m => m.Id)
                .Where(m => m.StorageId == storageId);
    }

    public record MetaData(int Id, string Name, float Rating, int StorageId);

    public class Query
    {
        public MetaData GetMetaData(
            int id,
            [Service] MetaDataRepository repository) =>
            repository.GetMetaData(id);

        public IEnumerable<MetaData> GetMetaDatasByStorage(
            [Service] MetaDataRepository repository,
            int id) => repository.GetMetaDataByStorageId(id);
    }
}