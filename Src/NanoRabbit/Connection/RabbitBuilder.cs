using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NanoRabbit.Connection;

public class ProducerOptionsBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<ProducerOptions> _producers;

    public ProducerOptionsBuilder(IServiceCollection services)
    {
        _services = services;
        _producers = new List<ProducerOptions>();
    }

    public void AddProducer(ProducerOptions options)
    {
        _producers.Add(options);
    }

    public RabbitProducerOptions Build()
    {
        return new RabbitProducerOptions
        {
            Producers = _producers
        };
    }
}

public class ConsumerOptionsBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<ConsumerOptions> _consumers;

    public ConsumerOptionsBuilder(IServiceCollection services)
    {
        _services = services;
        _consumers = new List<ConsumerOptions>();
    }

    public void AddConsumer(ConsumerOptions options)
    {
        _consumers.Add(options);
    }

    public RabbitConsumerOptions Build()
    {
        return new RabbitConsumerOptions
        {
            Consumers = _consumers
        };
    }
}

public static class ConfigurationExtensions
{
    /// <summary>
    /// Read NanoRabbit configs in appsettings.json
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static RabbitConfiguration? ReadSettings(this IConfiguration configuration)
    {
        var rabbitConfig = configuration.GetSection("NanoRabbit").Get<RabbitConfiguration>();
        return rabbitConfig;
    }
}