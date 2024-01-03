using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace NanoRabbit.DependencyInjection;

public static class RabbitConsumerExtensions
{
    public static IServiceCollection AddRabbitConsumer(this IServiceCollection services, Action<ConsumerOptionsBuilder> optionsBuilder)
    {
        var builder = new ConsumerOptionsBuilder(services);
        optionsBuilder.Invoke(builder);
        var options = builder.Build();
    
        services.AddScoped<IRabbitConsumer, RabbitConsumer>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<RabbitConsumer>>();
            var consumer = new RabbitConsumer(options.Consumers, logger);
            return consumer;
        });
    
        return services;
    }
}