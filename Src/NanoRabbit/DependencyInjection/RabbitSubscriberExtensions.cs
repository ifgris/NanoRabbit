using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace NanoRabbit.DependencyInjection;

public static class RabbitSubscriberExtensions
{
    public static IServiceCollection AddRabbitSubscriber<TSubscriber>(this IServiceCollection services,
        string consumerName, bool enableLogging = true) where TSubscriber : RabbitSubscriber
    {
        services.AddHostedService(provider =>
        {
            if (enableLogging)
            {
                var logger = provider.GetRequiredService<ILogger<RabbitSubscriber>>();
                var consumer = provider.GetRequiredService<IRabbitConsumer>();
                var subscriberService =
                    ActivatorUtilities.CreateInstance<TSubscriber>(provider, consumer, logger, consumerName);
                return subscriberService;
            }
            else
            {
                var consumer = provider.GetRequiredService<IRabbitConsumer>();
                var subscriberService =
                    ActivatorUtilities.CreateInstance<TSubscriber>(provider, consumer, consumerName);
                return subscriberService;
            }
        });

        return services;
    }
}