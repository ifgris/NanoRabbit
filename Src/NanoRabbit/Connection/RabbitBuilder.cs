using Microsoft.Extensions.DependencyInjection;

namespace NanoRabbit.Connection;

public class ProducerOptionsBuilder
{
    private readonly IServiceCollection _services;
    private List<ProducerOptions> _producers;

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
    private List<ConsumerOptions> _consumers;

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