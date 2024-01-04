using Microsoft.Extensions.Hosting;
using NanoRabbit.Consumer;

namespace Example.SimpleDI;

public class ConsumeService : RabbitSubscriber
{
    private readonly IRabbitConsumer _consumer;

    public ConsumeService(IRabbitConsumer consumer, string consumerName) : base(consumer, consumerName)
    {
        _consumer = consumer;
    }

    public override bool HandleMessage(string message)
    {
        Console.WriteLine(message);
        return true;
    }
}