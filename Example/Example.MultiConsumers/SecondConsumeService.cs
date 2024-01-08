using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.MultiConsumers;

public class SecondConsumeService : RabbitSubscriber
{
    private readonly IRabbitConsumer _consumer;

    public SecondConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriber>? logger) : base(consumer, logger)
    {
        _consumer = consumer;
        SetConsumer("FooSecondQueueConsumer");
    }
}