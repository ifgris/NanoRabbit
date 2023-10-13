using Microsoft.Extensions.DependencyInjection;
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

        services.AddScoped<RabbitConsumer>(_ =>
        {
            var consumer = new RabbitConsumer(options.Consumers);
            return consumer;
        });

        return services;
    }
}