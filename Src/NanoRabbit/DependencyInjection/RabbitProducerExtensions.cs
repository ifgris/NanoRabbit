using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace NanoRabbit.DependencyInjection;

public static class RabbitProducerExtensions
{
    public static IServiceCollection AddRabbitProducer(this IServiceCollection services, Action<ProducerOptionsBuilder> optionsBuilder)
    {
        var builder = new ProducerOptionsBuilder(services);
        optionsBuilder.Invoke(builder);
        var options = builder.Build();
    
        services.AddScoped<IRabbitProducer, RabbitProducer>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<RabbitProducer>>();
            var producer = new RabbitProducer(options.Producers, logger);
            return producer;
        });
    
        return services;
    }
}