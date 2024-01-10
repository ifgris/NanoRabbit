using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.Autofac;

public class ConsumeService : RabbitSubscriber
{
    public ConsumeService(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger = null) : base(consumer, consumerName, logger)
    {
    }

    protected override bool HandleMessage(string message)
    {
        Console.WriteLine(message);
        return true;
    }
}