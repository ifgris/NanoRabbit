using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Helper;
using RabbitMQ.Client;

namespace NanoRabbit.DependencyInjection;

/// <summary>
/// RabbitHelper extensions
/// </summary>
public static class RabbitHelperExtensions
{
    /// <summary>
    /// Add a singleton service of the type specified in IRabbitHelper with a factory specified in implementationFactory to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRabbitHelper(this IServiceCollection services)
    {
        var rabbitConfig = services.BuildServiceProvider().GetRequiredService<RabbitConfiguration>();
        if (rabbitConfig == null) throw new NullReferenceException("RabbitConfiguration not found");

        var connectionManager = services.BuildServiceProvider().GetService<IConnectionManager>();
        if (connectionManager == null) throw new NullReferenceException("ConnectionManager not registered");

        if (string.IsNullOrEmpty(rabbitConfig.ConnectionName))
            throw new NullReferenceException("RabbitConfiguration.ConnectionName not found");
        var factory = connectionManager.TryGetFactory(rabbitConfig.ConnectionName);

        if (factory == null)
        {
            factory = new ConnectionFactory
            {
                HostName = rabbitConfig.HostName,
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                VirtualHost = rabbitConfig.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            connectionManager.TryAddFactory(rabbitConfig.ConnectionName, factory);
        }

        var connection = connectionManager.TryConnectAsync(rabbitConfig.ConnectionName, factory).ConfigureAwait(false)
            .GetAwaiter().GetResult();
        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RabbitHelper>();

        services.AddSingleton<IRabbitHelper>(_ => new RabbitHelper(rabbitConfig, logger, connection));
        return services;
    }

    /// <summary>
    /// Add a keyed singleton service of the type specified in IRabbitHelper with a factory specified in implementationFactory to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key">An object that specifies the key of service object to get.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static IServiceCollection AddKeyedRabbitHelper(this IServiceCollection services, object? key)
    {
        var rabbitConfig = services.BuildServiceProvider().GetRequiredKeyedService<RabbitConfiguration>(key);
        if (rabbitConfig == null) throw new NullReferenceException("RabbitConfiguration not found");

        var connectionManager = services.BuildServiceProvider().GetRequiredKeyedService<IConnectionManager>(key);
        if (connectionManager == null) throw new NullReferenceException("ConnectionManager not registered");

        if (string.IsNullOrEmpty(rabbitConfig.ConnectionName))
            throw new NullReferenceException("RabbitConfiguration.ConnectionName not found");
        var factory = connectionManager.TryGetFactory(rabbitConfig.ConnectionName);

        if (factory == null)
        {
            factory = new ConnectionFactory
            {
                HostName = rabbitConfig.HostName,
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                VirtualHost = rabbitConfig.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            connectionManager.TryAddFactory(rabbitConfig.ConnectionName, factory);
        }

        var connection = connectionManager.TryConnectAsync(rabbitConfig.ConnectionName, factory).ConfigureAwait(false)
            .GetAwaiter().GetResult();
        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RabbitHelper>();

        services.AddKeyedSingleton<IRabbitHelper>(key, (_, _) =>
        {
            var rabbitHelper = new RabbitHelper(rabbitConfig, logger, connection);
            return rabbitHelper;
        });
        return services;
    }

    /// <summary>
    /// Add a singleton service of the type specified in IRabbitHelper by reading configurations of appsettings.json.
    /// </summary>
    /// <typeparam name="TRabbitConfiguration"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddRabbitHelperFromAppSettings<TRabbitConfiguration>(
        this IServiceCollection services, IConfiguration configuration)
        where TRabbitConfiguration : RabbitConfiguration, new()
    {
        TRabbitConfiguration? rabbitConfig = configuration.ReadSettings<TRabbitConfiguration>();
        if (rabbitConfig == null) throw new NullReferenceException("RabbitConfiguration not found");

        var connectionManager = services.BuildServiceProvider().GetService<IConnectionManager>();
        if (connectionManager == null) throw new NullReferenceException("ConnectionManager not registered");

        if (string.IsNullOrEmpty(rabbitConfig.ConnectionName))
            throw new NullReferenceException("RabbitConfiguration.ConnectionName not found");
        var factory = connectionManager.TryGetFactory(rabbitConfig.ConnectionName);

        if (factory == null)
        {
            factory = new ConnectionFactory
            {
                HostName = rabbitConfig.HostName,
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                VirtualHost = rabbitConfig.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            connectionManager.TryAddFactory(rabbitConfig.ConnectionName, factory);
        }

        var connection = connectionManager.TryConnectAsync(rabbitConfig.ConnectionName, factory).ConfigureAwait(false)
            .GetAwaiter().GetResult();
        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RabbitHelper>();

        services.AddSingleton<IRabbitHelper>(_ => new RabbitHelper(rabbitConfig, logger, connection));
        return services;
    }

    /// <summary>
    /// Add a keyed singleton service of the type specified in IRabbitHelper by reading configurations of appsettings.json.
    /// </summary>
    /// <typeparam name="TRabbitConfiguration"></typeparam>
    /// <param name="services"></param>
    /// <param name="key"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddKeyedRabbitHelperFromAppSettings<TRabbitConfiguration>(
        this IServiceCollection services, object? key, IConfiguration configuration)
        where TRabbitConfiguration : RabbitConfiguration, new()
    {
        TRabbitConfiguration? rabbitConfig = configuration.ReadSettings<TRabbitConfiguration>();
        if (rabbitConfig == null) throw new NullReferenceException("RabbitConfiguration not found");

        var connectionManager = services.BuildServiceProvider().GetRequiredKeyedService<IConnectionManager>(key);
        if (connectionManager == null) throw new NullReferenceException("ConnectionManager not registered");

        if (string.IsNullOrEmpty(rabbitConfig.ConnectionName))
            throw new NullReferenceException("RabbitConfiguration.ConnectionName not found");
        var factory = connectionManager.TryGetFactory(rabbitConfig.ConnectionName);

        if (factory == null)
        {
            factory = new ConnectionFactory
            {
                HostName = rabbitConfig.HostName,
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                VirtualHost = rabbitConfig.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            connectionManager.TryAddFactory(rabbitConfig.ConnectionName, factory);
        }

        var connection = connectionManager.TryConnectAsync(rabbitConfig.ConnectionName, factory).ConfigureAwait(false)
            .GetAwaiter().GetResult();

        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RabbitHelper>();

        services.AddSingleton<IRabbitHelper>(_ => new RabbitHelper(rabbitConfig, logger, connection));

        return services;
    }

    /// <summary>
    /// Get specific IRabbitHelper service by key.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IRabbitHelper GetRabbitHelper(this IServiceProvider serviceProvider, object? key)
    {
        return serviceProvider.GetRequiredKeyedService<IRabbitHelper>(key);
    }
}