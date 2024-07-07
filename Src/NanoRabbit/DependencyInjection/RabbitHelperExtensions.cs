using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NanoRabbit.Connection;

namespace NanoRabbit.DependencyInjection
{
    /// <summary>
    /// RabbitHelper extensions.
    /// </summary>
    public static class RabbitHelperExtensions
    {
        /// <summary>
        /// Adds a singleton service of the type specified in IRabbitHelper with a factory specified in implementationFactory to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitHelper(this IServiceCollection services, Action<RabbitConfigurationBuilder> builder)
        {
            var rabbitConfigBuilder = new RabbitConfigurationBuilder();
            builder.Invoke(rabbitConfigBuilder);
            var rabbitConfig = rabbitConfigBuilder.Build();

            services.AddSingleton<IRabbitHelper>(provider =>
            {
                return new RabbitHelper(rabbitConfig);
            });
            return services;
        }

        /// <summary>
        /// Adds a keyed singleton service of the type specified in IRabbitHelper with a factory specified in implementationFactory to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="key"></param>
        /// <param name="builders"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static IServiceCollection AddKeyedRabbitHelper(this IServiceCollection services, string key, Action<RabbitConfigurationBuilder> builders)
        {
#if NET7_0_OR_GREATER
            var rabbitConfigBuilder = new RabbitConfigurationBuilder();
            builders.Invoke(rabbitConfigBuilder);
            var rabbitConfig = rabbitConfigBuilder.Build();

            services.AddKeyedSingleton<IRabbitHelper>(key, (provider, _) =>
            {
                var rabbitHelper = new RabbitHelper(rabbitConfig);
                return rabbitHelper;
            });
            return services;
#else
            throw new NotSupportedException("Keyed services are only supported in .NET 7 and above.");
#endif
        }

        /// <summary>
        /// Adds a singleton service of the type specified in IRabbitHelper by reading configurations of appsettings.json.
        /// </summary>
        /// <typeparam name="TRabbitConfiguration"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IServiceCollection AddRabbitMqHelperFromAppSettings<TRabbitConfiguration>(this IServiceCollection services, IConfiguration configuration)
            where TRabbitConfiguration : RabbitConfiguration, new()
        {
            TRabbitConfiguration? rabbitConfig = ReadSettings<TRabbitConfiguration>(configuration);

            if (rabbitConfig != null)
            {
                services.AddSingleton<IRabbitHelper>(provider =>
                {
                    return new RabbitHelper(rabbitConfig);
                });
            }
            else
            {
                throw new Exception("NanoRabbit Configuration is incorrect.");
            }
            return services;
        }

        /// <summary>
        /// Adds a keyed singleton service of the type specified in IRabbitHelper by reading configurations of appsettings.json.
        /// </summary>
        /// <typeparam name="TRabbitConfiguration"></typeparam>
        /// <param name="services"></param>
        /// <param name="key"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IServiceCollection AddKeyedRabbitMqHelperFromAppSettings<TRabbitConfiguration>(this IServiceCollection services, string key, IConfiguration configuration)
            where TRabbitConfiguration : RabbitConfiguration, new()
        {
#if NET7_0_OR_GREATER
            TRabbitConfiguration? rabbitConfig = ReadSettings<TRabbitConfiguration>(configuration);

            if (rabbitConfig != null)
            {
                services.AddKeyedSingleton<IRabbitHelper>(key, (provider, _) =>
                {
                    var rabbitHelper = new RabbitHelper(rabbitConfig);
                    return rabbitHelper;
                });
            }
            else
            {
                throw new Exception("NanoRabbit Configuration is incorrect.");
            }
            return services;
#else
            throw new NotSupportedException("Keyed services are only supported in .NET 7 and above.");
#endif
        }

        /// <summary>
        /// Add a consumer to specific IRabbitHelper.
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="services"></param>
        /// <param name="consumerName"></param>
        /// <param name="consumers"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitConsumer<THandler>(this IServiceCollection services, string consumerName, int consumers = 1)
            where THandler : class, IMessageHandler
        {
            services.AddSingleton<THandler>();

            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqHelper = serviceProvider.GetRequiredService<IRabbitHelper>();
            var messageHandler = serviceProvider.GetRequiredService<THandler>();

            // todo: check if queue exists.
            // rabbitMqHelper.DeclareQueue(queueName);

            rabbitMqHelper.AddConsumer(consumerName, messageHandler.HandleMessage, consumers);

            return services;
        }

        /// <summary>
        /// Add a consumer to specific keyed IRabbitHelper.
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="services"></param>
        /// <param name="key"></param>
        /// <param name="consumerName"></param>
        /// <param name="consumers"></param>
        /// <returns></returns>
        public static IServiceCollection AddKeyedRabbitConsumer<THandler>(this IServiceCollection services, string key, string consumerName, int consumers = 1)
            where THandler : class, IMessageHandler
        {
#if NET7_0_OR_GREATER
            services.AddKeyedSingleton<THandler>(key);

            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqHelper = serviceProvider.GetRequiredKeyedService<IRabbitHelper>(key);
            var messageHandler = serviceProvider.GetRequiredKeyedService<THandler>(key);

            // todo: check if queue exists.
            // rabbitMqHelper.DeclareQueue(queueName);

            rabbitMqHelper.AddConsumer(consumerName, messageHandler.HandleMessage, consumers);

            return services;
#else
            throw new NotSupportedException("Keyed services are only supported in .NET 7 and above.");
#endif
        }

        /// <summary>
        /// Add a async consumer to specific IRabbitHelper.
        /// </summary>
        /// <typeparam name="TAsyncHandler"></typeparam>
        /// <param name="services"></param>
        /// <param name="consumerName"></param>
        /// <param name="consumers"></param>
        /// <returns></returns>
        public static IServiceCollection AddAsyncRabbitConsumer<TAsyncHandler>(this IServiceCollection services, string consumerName, int consumers = 1)
        where TAsyncHandler : class, IAsyncMessageHandler
        {
            services.AddSingleton<TAsyncHandler>();

            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqHelper = serviceProvider.GetRequiredService<IRabbitHelper>();
            var messageHandler = serviceProvider.GetRequiredService<TAsyncHandler>();

            // todo: check if queue exists.
            // rabbitMqHelper.DeclareQueue(queueName);

            rabbitMqHelper.AddAsyncConsumer(consumerName, messageHandler.HandleMessageAsync, consumers);

            return services;
        }

        /// <summary>
        /// Add a async consumer to specific keyed IRabbitHelper.
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="services"></param>
        /// <param name="key"></param>
        /// <param name="consumerName"></param>
        /// <param name="consumers"></param>
        /// <returns></returns>
        public static IServiceCollection AddKeyedAsyncRabbitConsumer<TAsyncHandler>(this IServiceCollection services, string key, string consumerName, int consumers = 1)
            where TAsyncHandler : class, IAsyncMessageHandler
        {
#if NET7_0_OR_GREATER
            services.AddKeyedSingleton<TAsyncHandler>(key);

            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqHelper = serviceProvider.GetRequiredKeyedService<IRabbitHelper>(key);
            var messageHandler = serviceProvider.GetRequiredKeyedService<TAsyncHandler>(key);

            // todo: check if queue exists.
            // rabbitMqHelper.DeclareQueue(queueName);

            rabbitMqHelper.AddAsyncConsumer(consumerName, messageHandler.HandleMessageAsync, consumers);

            return services;
#else
            throw new NotSupportedException("Keyed services are only supported in .NET 7 and above.");
#endif
        }

        /// <summary>
        /// Get specific IRabbitHelper service by key.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IRabbitHelper GetRabbitHelper(this IServiceProvider serviceProvider, string key)
        {
#if NET7_0_OR_GREATER
            return serviceProvider.GetRequiredKeyedService<IRabbitHelper>(key);
#else
            throw new NotSupportedException("Keyed services are only supported in .NET 7 and above.");
#endif
        }

        /// <summary>
        /// Read NanoRabbit configs in appsettings.json
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static TRabbitConfiguration? ReadSettings<TRabbitConfiguration>(this IConfiguration configuration)
        {
            var configSection = configuration.GetSection(typeof(TRabbitConfiguration).Name);
            if (!configSection.Exists())
            {
                throw new Exception($"Configuration section '{typeof(TRabbitConfiguration).Name}' not found.");
            }
            var rabbitConfig = configSection.Get<TRabbitConfiguration>();
            return rabbitConfig;
        }
    }
}
