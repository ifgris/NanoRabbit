using Microsoft.Extensions.DependencyInjection;
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
    
        services.AddScoped<RabbitProducer>(_ =>
        {
            var producer = new RabbitProducer(options.Producers);
            return producer;
        });
    
        return services;
    }
}