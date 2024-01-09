using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace NanoRabbit.DependencyInjection;

public static class RabbitProducerExtensions
{
    public static IServiceCollection AddRabbitProducer(this IServiceCollection services, Action<ProducerOptionsBuilder> optionsBuilder, bool enableLogging = true)
    {
        var builder = new ProducerOptionsBuilder(services);
        optionsBuilder.Invoke(builder);
        var options = builder.Build();
    
        services.AddScoped<IRabbitProducer, RabbitProducer>(provider =>
        {
            if (enableLogging)
            {
                var logger = provider.GetRequiredService<ILogger<RabbitProducer>>();
                var producer = new RabbitProducer(options.Producers, logger);
                return producer;
            }
            else
            {
                var producer = new RabbitProducer(options.Producers);
                return producer;
            }
        });
    
        return services;
    }
    
    public static IServiceCollection AutoAddRabbitProducer(this IServiceCollection services, IConfiguration configuration, bool enableLogging = true)
    {
        var rabbitConfig = configuration.ReadSettings();
        var producerList = rabbitConfig?.Producers;
    
        services.AddScoped<IRabbitProducer, RabbitProducer>(provider =>
        {
            if (enableLogging && producerList != null)
            {
                var logger = provider.GetRequiredService<ILogger<RabbitProducer>>();
                var producer = new RabbitProducer(producerList, logger);
                return producer;
            }
            
            if (!enableLogging && producerList != null)
            {
                var producer = new RabbitProducer(producerList);
                return producer;
            }

            throw new Exception("No producers detected in appsettings.json");
        });
    
        return services;
    }
}