using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NanoRabbit.Connection;
using NanoRabbit.Helper;
using NanoRabbit.Helper.MessageHandler;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitHelperExtensions
    {
        public static IServiceCollection AddRabbitHelper(this IServiceCollection services, Action<RabbitConfigurationBuilder> builder)
        {
            var rabbitConfigBuilder = new RabbitConfigurationBuilder(services);
            builder.Invoke(rabbitConfigBuilder);
            var rabbitConfig = rabbitConfigBuilder.Build();

            services.AddSingleton<IRabbitHelper>(sp => new RabbitHelper(rabbitConfig));
            return services;
        }

        public static IServiceCollection AddKeyedRabbitHelper(this IServiceCollection services, string key, Action<RabbitConfigurationBuilder> builders)
        {
            var rabbitConfigBuilder = new RabbitConfigurationBuilder(services);
            builders.Invoke(rabbitConfigBuilder);
            var rabbitConfig = rabbitConfigBuilder.Build();

            services.AddKeyedSingleton<IRabbitHelper>(key, (key, provider) =>
            {
                var rabbitHelper = new RabbitHelper(rabbitConfig);
                return rabbitHelper;
            });

            return services;
        }

        public static IServiceCollection AddRabbitMqHelperFromAppSettings<TRabbitConfiguration>(this IServiceCollection services, IConfiguration configuration)
            where TRabbitConfiguration : RabbitConfiguration, new()
        {
            var configSection = configuration.GetSection(typeof(TRabbitConfiguration).Name);
            if (!configSection.Exists())
            {
                throw new Exception($"Configuration section '{typeof(TRabbitConfiguration).Name}' not found.");
            }
            var rabbitConfig = configSection.Get<TRabbitConfiguration>();

            services.AddSingleton<IRabbitHelper>(sp => new RabbitHelper(rabbitConfig));
            return services;
        }
        
        public static IServiceCollection AddKeyedRabbitMqHelperFromAppSettings<TRabbitConfiguration>(this IServiceCollection services, string key, IConfiguration configuration)
            where TRabbitConfiguration : RabbitConfiguration, new()
        {
            var configSection = configuration.GetSection(typeof(TRabbitConfiguration).Name);
            if (!configSection.Exists())
            {
                throw new Exception($"Configuration section '{typeof(TRabbitConfiguration).Name}' not found.");
            }
            var rabbitConfig = configSection.Get<TRabbitConfiguration>();

            services.AddKeyedSingleton<IRabbitHelper>(key, (key, provider) => new RabbitHelper(rabbitConfig));
            return services;
        }

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
        
        public static IServiceCollection AddKeyedRabbitConsumer<THandler>(this IServiceCollection services, string key, string consumerName, int consumers = 1)
            where THandler : class, IMessageHandler
        {
            services.AddKeyedSingleton<THandler>(key);

            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqHelper = serviceProvider.GetRequiredKeyedService<IRabbitHelper>(key);
            var messageHandler = serviceProvider.GetRequiredKeyedService<THandler>(key);

            // todo: check if queue exists.
            // rabbitMqHelper.DeclareQueue(queueName);

            rabbitMqHelper.AddConsumer(consumerName, messageHandler.HandleMessage, consumers);

            return services;
        }

        public static IServiceCollection AddAsyncRabbitConsumer<TAsyncHandler>(this IServiceCollection services, string consumerName, int consumers = 1)
        where TAsyncHandler : class, IAsyncMessageHandler
        {
            services.AddSingleton<TAsyncHandler>();

            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqHelper = serviceProvider.GetRequiredService<IRabbitHelper>();
            var messageHandler = serviceProvider.GetRequiredService<TAsyncHandler>();

            // todo: check if queue exists.
            // rabbitMqHelper.DeclareQueue(queueName);

            rabbitMqHelper.AddAsyncConsumer(consumerName, messageHandler.HandleMessageAsync, consumers).GetAwaiter().GetResult();

            return services;
        }
    }
}
