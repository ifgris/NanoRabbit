using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Helper;
using NanoRabbit.Service;

namespace NanoRabbit.DependencyInjection;

/// <summary>
/// Rabbit Consumer Services Extensions
/// </summary>
public static class RabbitConsumerServiceExtensions
{
    /// <summary>
    /// Add RabbitConsumerService(RabbitAsyncConsumerService) BackgroundService for each consumer configs.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>Configured IServiceCollection</returns>
    /// <exception cref="ArgumentNullException">Throws when configuration is null.</exception>
    public static IServiceCollection AddRabbitConsumer(
        this IServiceCollection services)
    {
        
        var configuration = services.BuildServiceProvider().GetRequiredService<RabbitConfiguration>();

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (configuration.Consumers == null)
        {
            throw new ArgumentNullException(nameof(ConsumerOptions));
        }

        // Register all consumers
        foreach (var consumerOptions in configuration.Consumers)
        {
            // Verify that the ConsumerOptions are valid
            if (string.IsNullOrWhiteSpace(consumerOptions.ConsumerName))
            {
                Console.WriteLine($"[Warning] Host '{configuration.HostName}' missing ConsumerName, skipping.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(consumerOptions.QueueName))
            {
                Console.WriteLine(
                    $"[Warning] Host '{configuration.HostName}'-'{consumerOptions.ConsumerName}' missing QueueName, skipping.");
                continue;
            }

            // Ensure that the HandlerName exists, as it is needed by the consumer service to resolve the handler
            if (string.IsNullOrWhiteSpace(consumerOptions.HandlerName))
            {
                Console.WriteLine(
                    $"[Warning] Host '{configuration.HostName}'-'{consumerOptions.ConsumerName}' missing HandlerName, The HandlerName is the name of the IMessageHandler. Be sure to configure each consumer with a HandlerIdentifier in order to resolve the corresponding IMessageHandler.");
                continue;
            }
            
            var connectionManager = services.BuildServiceProvider().GetRequiredService<IConnectionManager>();
            
            // Register as IHostedService for each consumer
            for (int i = 0; i < consumerOptions.ConsumerCount; i++)
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<RabbitAsyncConsumerService<RabbitConfiguration>>();

                    return new RabbitAsyncConsumerService<RabbitConfiguration>(
                        logger,
                        configuration,
                        consumerOptions.ConsumerName,
                        provider,
                        connectionManager,
                        configuration.ConnectionName ?? throw new ArgumentException("ConnectionName is missing")
                    );
                });

                Console.WriteLine(
                    $"'{consumerOptions.ConsumerName}' (Host: {configuration.HostName}, Queue: {consumerOptions.QueueName}) has been registered.");
            }
        }

        return services;
    }

    /// <summary>
    /// Add Keyed RabbitConsumerService(RabbitAsyncConsumerService) BackgroundService for each consumer configs.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="key">An object that specifies the key of service object to get.</param>
    /// <returns>Configured IServiceCollection</returns>
    /// <exception cref="ArgumentNullException">Throws when configuration is null.</exception>
    public static IServiceCollection AddKeyedRabbitConsumer(
        this IServiceCollection services, object? key)
    {
        
        var configuration = services.BuildServiceProvider().GetRequiredKeyedService<RabbitConfiguration>(key);

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (configuration.Consumers == null)
        {
            throw new ArgumentNullException(nameof(ConsumerOptions));
        }

        // Register all consumers
        foreach (var consumerOptions in configuration.Consumers)
        {
            // Verify that the ConsumerOptions are valid
            if (string.IsNullOrWhiteSpace(consumerOptions.ConsumerName))
            {
                Console.WriteLine($"[Warning] Host '{configuration.HostName}' missing ConsumerName, skipping.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(consumerOptions.QueueName))
            {
                Console.WriteLine(
                    $"[Warning] Host '{configuration.HostName}'-'{consumerOptions.ConsumerName}' missing QueueName, skipping.");
                continue;
            }

            // Ensure that the HandlerIdentifier exists, as it is needed by the consumer service to resolve the handler
            if (string.IsNullOrWhiteSpace(consumerOptions.HandlerName))
            {
                Console.WriteLine(
                    $"[Warning] Host '{configuration.HostName}'-'{consumerOptions.ConsumerName}' missing HandlerName, The HandlerName is the name of the IMessageHandler. Be sure to configure each consumer with a HandlerName in order to resolve the corresponding IMessageHandler.");
                continue;
            }

            // Register as IHostedService for each consumer
            for (int i = 0; i < consumerOptions.ConsumerCount; i++)
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<RabbitAsyncConsumerService<RabbitConfiguration>>();
                    var connectionManager = provider.GetRequiredService<IConnectionManager>();

                    return new RabbitAsyncConsumerService<RabbitConfiguration>(
                        logger,
                        configuration,
                        consumerOptions.ConsumerName,
                        provider,
                        connectionManager,
                        configuration.ConnectionName ?? throw new ArgumentException("ConsumerName is missing")
                    );
                });

                Console.WriteLine(
                    $"'{consumerOptions.ConsumerName}' (Host: {configuration.HostName}, Queue: {consumerOptions.QueueName}) has been registered.");
            }
        }

        return services;
    }

    /// <summary>
    /// Add Keyed RabbitConsumerService(RabbitAsyncConsumerService) BackgroundService for each consumer configs by reading appsettings.json.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configuration"></param>
    /// <returns>Configured IServiceCollection</returns>
    /// <exception cref="ArgumentNullException">Throws when configuration is null.</exception>
    public static IServiceCollection AddRabbitConsumerFromAppSettings<TRabbitConfiguration>(
        this IServiceCollection services, IConfiguration configuration)
        where TRabbitConfiguration : RabbitConfiguration, new()
    {
        
        TRabbitConfiguration? rabbitConfig = configuration.ReadSettings<TRabbitConfiguration>();

        if (rabbitConfig == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (rabbitConfig.Consumers == null)
        {
            throw new ArgumentNullException(nameof(ConsumerOptions));
        }

        // Register all consumers
        foreach (var consumerOptions in rabbitConfig.Consumers)
        {
            // Verify that the ConsumerOptions are valid
            if (string.IsNullOrWhiteSpace(consumerOptions.ConsumerName))
            {
                Console.WriteLine($"[Warning] Host '{rabbitConfig.HostName}' missing ConsumerName, skipping.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(consumerOptions.QueueName))
            {
                Console.WriteLine(
                    $"[Warning] Host '{rabbitConfig.HostName}'-'{consumerOptions.ConsumerName}' missing QueueName, skipping.");
                continue;
            }

            // Ensure that the HandlerName exists, as it is needed by the consumer service to resolve the handler
            if (string.IsNullOrWhiteSpace(consumerOptions.HandlerName))
            {
                Console.WriteLine(
                    $"[Warning] Host '{rabbitConfig.HostName}'-'{consumerOptions.ConsumerName}' missing HandlerName, The HandlerName is the name of the IMessageHandler. Be sure to configure each consumer with a HandlerName in order to resolve the corresponding IMessageHandler.");
                continue;
            }

            // Register as IHostedService for each consumer
            for (int i = 0; i < consumerOptions.ConsumerCount; i++)
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<RabbitAsyncConsumerService<RabbitConfiguration>>();
                    var connectionManager = provider.GetRequiredService<IConnectionManager>();

                    return new RabbitAsyncConsumerService<RabbitConfiguration>(
                        logger,
                        rabbitConfig,
                        consumerOptions.ConsumerName,
                        provider,
                        connectionManager,
                        rabbitConfig.ConnectionName ?? throw new ArgumentException("ConsumerName is missing")
                    );
                });

                Console.WriteLine(
                    $"'{consumerOptions.ConsumerName}' (Host: {rabbitConfig.HostName}, Queue: {consumerOptions.QueueName}) has been registered.");
            }
        }

        return services;
    }
}