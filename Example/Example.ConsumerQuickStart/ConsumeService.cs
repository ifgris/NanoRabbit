using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.ConsumerQuickStart;

public class ConsumeService : RabbitSubscriber
{
    public ConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriber>? logger) : base(consumer, logger)
    {
        SetConsumer("FooSecondQueueConsumer");
    }

    // protected override bool HandleMessage(string message)
    // {
    //     _logger?.LogInformation(message);
    //     return true;
    // }
}