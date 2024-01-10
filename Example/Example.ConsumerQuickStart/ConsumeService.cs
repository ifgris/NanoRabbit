using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.ConsumerQuickStart;

public class ConsumeService : RabbitSubscriber
{
    public ConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriber>? logger, string consumerName) : base(consumer, consumerName, logger)
    {
    }

    protected override bool HandleMessage(string message)
    {
        Console.WriteLine(message);
        return true;
    }
}