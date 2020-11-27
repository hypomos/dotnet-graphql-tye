using Contract;
using HotChocolate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
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
                .AddSingleton<UserRepository>()
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .InitializeOnStartup()
                .PublishSchemaDefinition(c => c
                    .SetName(Constants.UserApi)
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

    public class UserRepository
    {
        private readonly Dictionary<int, User> _user;

        public UserRepository()
        {
            _user = new User[]
            {
                new User(1, "some.sample@example.com", "Some", "Sample"),
                new User(2, "another.sample@example.com", "Another", "Sample"),
                new User(3, "anderson.smith@example.com", "Anderson", "Smith"),
            }.ToDictionary(t => t.Id);
        }

        public User GetUser(int id) => _user[id];
    }

    public record User(int Id, string Email, string Firstname, string Lastname);

    public class Query
    {
        public User GetUser(
            int id,
            [Service] UserRepository repository) =>
            repository.GetUser(id);
    }
}