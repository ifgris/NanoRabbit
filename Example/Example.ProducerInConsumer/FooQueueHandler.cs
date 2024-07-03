using NanoRabbit.Helper;
using NanoRabbit.Helper.MessageHandler;

namespace Example.ProducerInConsumer;

public class FooQueueHandler : DefaultMessageHandler
{
    private readonly IRabbitHelper _rabbitHelper;

    public FooQueueHandler(IRabbitHelper rabbitHelper)
    {
        _rabbitHelper = rabbitHelper;
    }

    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from foo-queue: {message}");

        _rabbitHelper.Publish("BarProducer", $"forwared from foo-queue: {message}");

        Console.WriteLine("Forwarded a message from foo-queue");

        Console.WriteLine("[x] Done");
    }
}