using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace NanoRabbit.DependencyInjection;

public static class RabbitConsumerExtensions
{
    public static IServiceCollection AddRabbitConsumer(this IServiceCollection services,
        Action<ConsumerOptionsBuilder> optionsBuilder, bool enableLogging = true)
    {
        var builder = new ConsumerOptionsBuilder(services);
        optionsBuilder.Invoke(builder);
        var options = builder.Build();
    
        services.AddScoped<IRabbitConsumer, RabbitConsumer>(provider =>
        {
            if (enableLogging)
            {
                var logger = provider.GetRequiredService<ILogger<RabbitConsumer>>();
                var consumer = new RabbitConsumer(options.Consumers, logger);
                return consumer;
            }
            else
            {
                var consumer = new RabbitConsumer(options.Consumers);
                return consumer;
            }
        });
    
        return services;
    }
    
    public static IServiceCollection AddRabbitConsumerFromAppSettings(this IServiceCollection services, IConfiguration configuration, bool enableLogging = true)
    {
        var rabbitConfig = configuration.ReadSettings();
        var consumerList = rabbitConfig?.Consumers;
    
        services.AddScoped<IRabbitConsumer, RabbitConsumer>(provider =>
        {
            if (enableLogging && consumerList != null)
            {
                var logger = provider.GetRequiredService<ILogger<RabbitConsumer>>();
                var consumer = new RabbitConsumer(consumerList, logger);
                return consumer;
            }

            if (!enableLogging && consumerList != null)
            {
                var consumer = new RabbitConsumer(consumerList);
                return consumer;
            }

            throw new Exception("No consumers detected in appsettings.json");
        });
    
        return services;
    }
}