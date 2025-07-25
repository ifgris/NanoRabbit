using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitConnectionExtensions
    {
        public static IServiceCollection AddRabbitConnection(this IServiceCollection services,
            Action<RabbitConfigurationBuilder> builder)
        {
            var rabbitConfigBuilder = new RabbitConfigurationBuilder();
            builder.Invoke(rabbitConfigBuilder);
            var rabbitConfig = rabbitConfigBuilder.Build();
            
            // Register connections and factories
            services.TryAddSingleton(x =>
            {
                var connections = new ConcurrentDictionary<string, Task<IConnection?>>();
                return connections;
            });
            services.TryAddSingleton(x =>
            {
                var factories = new ConcurrentDictionary<string, ConnectionFactory>();
                return factories;
            });
            
            // Register IConnectionManager
            services.AddSingleton<IConnectionManager, ConnectionManager>(x =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ConnectionManager>();
                var connections = x.GetRequiredService<ConcurrentDictionary<string, Task<IConnection?>>>();
                var factories = x.GetRequiredService<ConcurrentDictionary<string, ConnectionFactory>>();
                var connectionManager = new ConnectionManager(logger, connections, factories);
                return connectionManager;
            });
            
            return services;
        }
        
        public static IServiceCollection AddRabbitConnectionFromAppSettings(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register connections and factories
            services.TryAddSingleton(x =>
            {
                var connections = new ConcurrentDictionary<string, Task<IConnection?>>();
                return connections;
            });
            services.TryAddSingleton(x =>
            {
                var factories = new ConcurrentDictionary<string, ConnectionFactory>();
                return factories;
            });
            
            // Register IConnectionManager
            services.AddSingleton<IConnectionManager, ConnectionManager>(x =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ConnectionManager>();
                var connections = x.GetRequiredService<ConcurrentDictionary<string, Task<IConnection?>>>();
                var factories = x.GetRequiredService<ConcurrentDictionary<string, ConnectionFactory>>();
                var connectionManager = new ConnectionManager(logger, connections, factories);
                return connectionManager;
            });
            
            return services;
        }
    }
}