using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Storage
{
    using Contract;
    using HotChocolate;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
                .AddSingleton<StorageRepository>()
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .InitializeOnStartup()
                .PublishSchemaDefinition(c => c
                    .SetName(Constants.StorageApi)
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

    public class StorageRepository
    {
        private readonly Dictionary<int, Storage> _Storages;

        public StorageRepository()
        {
            _Storages = new Storage[]
            {
                new Storage(1, "OneDrive", 1),
                new Storage(2, "Azure Blob Storage", 1)
            }.ToDictionary(t => t.Id);
        }

        public Storage GetStorage(int id) => _Storages[id];

        public IEnumerable<Storage> GetStorages() => _Storages.Values;

        public IEnumerable<Storage> GetStorageByUser(int id) => _Storages.Values.OrderBy(s => s.Id).Where(s => s.OwnerId == id);
    }

    public record Storage(int Id, string Name, int OwnerId);

    public class Query
    {
        public IEnumerable<Storage> GetStorages([Service] StorageRepository repository) =>
            repository.GetStorages();

        public Storage GetStorage(int id, [Service] StorageRepository repository) =>
            repository.GetStorage(id);

        public IEnumerable<Storage> GetStoragesByUser(int id, [Service] StorageRepository repository) =>
            repository.GetStorageByUser(id);
    }
}