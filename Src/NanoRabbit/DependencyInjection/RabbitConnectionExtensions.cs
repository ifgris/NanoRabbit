using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NanoRabbit.Helper;
using RabbitMQ.Client;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitConnectionExtensions
    {
        public static IServiceCollection AddRabbitConnection(this IServiceCollection services,
            Action<RabbitConfigurationBuilder> builder)
        {
            // Get RabbitConfiguration
            var rabbitConfigBuilder = new RabbitConfigurationBuilder();
            builder.Invoke(rabbitConfigBuilder);
            var rabbitConfig = rabbitConfigBuilder.Build();
            
            // Register RabbitConfiguration
            services.AddSingleton(rabbitConfig);
            
            // Register connections and factories
            services.TryAddSingleton(_ =>
            {
                var connections = new ConcurrentDictionary<string, Task<IConnection?>>();
                return connections;
            });
            if (string.IsNullOrEmpty(rabbitConfig.ConnectionName)) throw new NullReferenceException("ConnectionName");
            services.TryAddSingleton(_ =>
            {
                var factories = new ConcurrentDictionary<string, ConnectionFactory>();
                factories.TryAdd(rabbitConfig.ConnectionName, new ConnectionFactory
                {
                    HostName = rabbitConfig.HostName,
                    Port = rabbitConfig.Port,
                    UserName = rabbitConfig.UserName,
                    Password = rabbitConfig.Password,
                    VirtualHost = rabbitConfig.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                });
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
        
        public static IServiceCollection AddKeyedRabbitConnection(this IServiceCollection services, object? key,
            Action<RabbitConfigurationBuilder> builder)
        {
            // Get RabbitConfiguration
            var rabbitConfigBuilder = new RabbitConfigurationBuilder();
            builder.Invoke(rabbitConfigBuilder);
            var rabbitConfig = rabbitConfigBuilder.Build();
            
            // Register RabbitConfiguration
            services.AddKeyedSingleton(key, rabbitConfig);
            
            // Register connections and factories
            services.TryAddKeyedSingleton(key, (_, _) =>
            {
                var connections = new ConcurrentDictionary<string, Task<IConnection?>>();
                return connections;
            });
            if (string.IsNullOrEmpty(rabbitConfig.ConnectionName)) throw new NullReferenceException("ConnectionName");
            services.TryAddKeyedSingleton(key, (_, _) =>
            {
                var factories = new ConcurrentDictionary<string, ConnectionFactory>();
                factories.TryAdd(rabbitConfig.ConnectionName, new ConnectionFactory
                {
                    HostName = rabbitConfig.HostName,
                    Port = rabbitConfig.Port,
                    UserName = rabbitConfig.UserName,
                    Password = rabbitConfig.Password,
                    VirtualHost = rabbitConfig.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                });
                return factories;
            });
            
            // Register IConnectionManager
            services.AddKeyedSingleton<IConnectionManager, ConnectionManager>(key, (x, _) =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ConnectionManager>();
                var connections = x.GetRequiredKeyedService<ConcurrentDictionary<string, Task<IConnection?>>>(key);
                var factories = x.GetRequiredKeyedService<ConcurrentDictionary<string, ConnectionFactory>>(key);
                var connectionManager = new ConnectionManager(logger, connections, factories);
                return connectionManager;
            });
            
            return services;
        }
        
        public static IServiceCollection AddRabbitConnectionFromAppSettings<TRabbitConfiguration>(this IServiceCollection services,
            IConfiguration configuration)
        where TRabbitConfiguration : RabbitConfiguration, new()
        {
            // Get RabbitConfiguration
            TRabbitConfiguration? rabbitConfig = configuration.ReadSettings<TRabbitConfiguration>();
            if  (rabbitConfig == null) throw new NullReferenceException("TRabbitConfiguration");
            
            // Register RabbitConfiguration
            services.TryAddSingleton(rabbitConfig);
            
            // Register connections and factories
            services.TryAddSingleton(_ =>
            {
                var connections = new ConcurrentDictionary<string, Task<IConnection?>>();
                return connections;
            });
            if (string.IsNullOrEmpty(rabbitConfig.ConnectionName)) throw new NullReferenceException("ConnectionName");
            services.TryAddSingleton(_ =>
            {
                var factories = new ConcurrentDictionary<string, ConnectionFactory>();
                factories.TryAdd(rabbitConfig.ConnectionName, new ConnectionFactory
                {
                    HostName = rabbitConfig.HostName,
                    Port = rabbitConfig.Port,
                    UserName = rabbitConfig.UserName,
                    Password = rabbitConfig.Password,
                    VirtualHost = rabbitConfig.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                });
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
        
        public static IServiceCollection AddKeyedRabbitConnectionFromAppSettings<TRabbitConfiguration>(this IServiceCollection services, object? key, 
            IConfiguration configuration)
        where TRabbitConfiguration : RabbitConfiguration
        {
            // Get RabbitConfiguration
            TRabbitConfiguration? rabbitConfig = configuration.ReadSettings<TRabbitConfiguration>();
            if (rabbitConfig == null) throw new NullReferenceException("TRabbitConfiguration");
            
            // Register RabbitConfiguration
            services.TryAddKeyedSingleton(key, rabbitConfig);
            
            // Register connections and factories
            services.TryAddKeyedSingleton(key, (_, _) =>
            {
                var connections = new ConcurrentDictionary<string, Task<IConnection?>>();
                return connections;
            });
            if (string.IsNullOrEmpty(rabbitConfig.ConnectionName)) throw new NullReferenceException("ConnectionName");
            services.TryAddKeyedSingleton(key, (_, _) =>
            {
                var factories = new ConcurrentDictionary<string, ConnectionFactory>();
                factories.TryAdd(rabbitConfig.ConnectionName, new ConnectionFactory
                {
                    HostName = rabbitConfig.HostName,
                    Port = rabbitConfig.Port,
                    UserName = rabbitConfig.UserName,
                    Password = rabbitConfig.Password,
                    VirtualHost = rabbitConfig.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                });
                return factories;
            });
            
            // Register IConnectionManager
            services.AddKeyedSingleton<IConnectionManager, ConnectionManager>(key, (x, _) =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ConnectionManager>();
                var connections = x.GetRequiredKeyedService<ConcurrentDictionary<string, Task<IConnection?>>>(key);
                var factories = x.GetRequiredKeyedService<ConcurrentDictionary<string, ConnectionFactory>>(key);
                var connectionManager = new ConnectionManager(logger, connections, factories);
                return connectionManager;
            });
            
            return services;
        }
    }
}