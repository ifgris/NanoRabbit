using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace NanoRabbit.DependencyInjection;

public static class RabbitSubscriberExtensions
{
    public static IServiceCollection AddRabbitSubscriber<TSubscriber>(this IServiceCollection services,
        string consumerName, int consumerCount = 1, bool enableLogging = true) where TSubscriber : RabbitSubscriber
    {
        services.AddHostedService(provider =>
        {
            if (enableLogging)
            {
                var logger = provider.GetRequiredService<ILogger<RabbitSubscriber>>();
                var consumer = provider.GetRequiredService<IRabbitConsumer>();
                return ActivatorUtilities.CreateInstance<TSubscriber>(provider, consumer, logger, consumerName, consumerCount);
            }
            else
            {
                var consumer = provider.GetRequiredService<IRabbitConsumer>();
                return ActivatorUtilities.CreateInstance<TSubscriber>(provider, consumer, consumerName, consumerCount);
            }
        });

        return services;
    }
    
    public static IServiceCollection AddRabbitAsyncSubscriber<TAsyncSubscriber>(this IServiceCollection services,
        string consumerName, int consumerCount = 1, bool enableLogging = true) where TAsyncSubscriber : RabbitAsyncSubscriber
    {
        services.AddHostedService(provider =>
        {
            if (enableLogging)
            {
                var logger = provider.GetRequiredService<ILogger<RabbitAsyncSubscriber>>();
                var consumer = provider.GetRequiredService<IRabbitConsumer>();
                return ActivatorUtilities.CreateInstance<TAsyncSubscriber>(provider, consumer, logger, consumerName, consumerCount);
            }
            else
            {
                var consumer = provider.GetRequiredService<IRabbitConsumer>();
                return ActivatorUtilities.CreateInstance<TAsyncSubscriber>(provider, consumer, consumerName, consumerCount);
            }
        });

        return services;
    }
}

public class CompositeHostedService : IHostedService
{
    private readonly List<IHostedService> _hostedServices;

    public CompositeHostedService(List<IHostedService> hostedServices)
    {
        _hostedServices = hostedServices;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var tasks = _hostedServices.Select(service => service.StartAsync(cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var tasks = _hostedServices.Select(service => service.StopAsync(cancellationToken));
        await Task.WhenAll(tasks);
    }
}
