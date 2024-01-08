using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;
using NanoRabbit.Producer;

namespace Example.ProducerInConsumer;

public class ConsumeService : RabbitSubscriber
{
    private readonly IRabbitProducer _producer;

    public ConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriber>? logger, IRabbitProducer producer) : base(consumer, logger)
    {
        _producer = producer;
        SetConsumer("BarFirstQueueConsumer");
    }

    protected override bool HandleMessage(string message)
    {
        _producer.Publish("FooSecondQueueProducer", message);
        return true;
    }
}