using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Helper;
using NanoRabbit.Helper.MessageHandler;

var rabbitHelper = new RabbitHelper(rabbitConfig: new RabbitConfiguration
{
    HostName = "localhost",
    Port = 5672,
    VirtualHost = "/",
    UserName = "admin",
    Password = "admin",
    Consumers = new List<ConsumerOptions>
    {
        new ConsumerOptions
        {
            ConsumerName = "FooConsumer",
            QueueName = "foo-queue"
        },
        new ConsumerOptions
        {
            ConsumerName = "BarConsumer",
            QueueName = "bar-queue"
        }
    }
});

var fooHandler = new FooQueueHandler();
var barHandler = new BarQueueHandler();

rabbitHelper.AddConsumer("FooConsumer", fooHandler.HandleMessage);
rabbitHelper.AddConsumer("FooConsumer", barHandler.HandleMessage);

public class FooQueueHandler : DefaultMessageHandler
{
    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from foo-queue: {message}");
        // 自定义处理逻辑
        Task.Delay(1000).Wait();
        Console.WriteLine("[x] Done");
    }
}

public class BarQueueHandler : DefaultMessageHandler
{
    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from bar-queue: {message}");
        // 自定义处理逻辑
        Task.Delay(500).Wait();
        Console.WriteLine("[x] Done");
    }
}