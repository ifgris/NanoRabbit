using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddRabbitHelper(builder =>
        {
            builder.SetHostName("localhost");
            builder.SetPort(5672);
            builder.SetVirtualHost("/");
            builder.SetUserName("admin");
            builder.SetPassword("admin");
            builder.EnableLogging(true);
            builder.AddProducer(new ProducerOptions
            {
                ProducerName = "FooProducer",
                ExchangeName = "amq.topic",
                RoutingKey = "foo.key",
                Type = ExchangeType.Topic
            });
            builder.AddConsumer(new ConsumerOptions
            {
                ConsumerName = "FooConsumer",
                QueueName = "foo-queue"
            });
            builder.AddConsumer(new ConsumerOptions
            {
                ConsumerName = "BarConsumer",
                QueueName = "bar-queue"
            });
        })
        .AddRabbitConsumer<FooQueueHandler>("FooConsumer", consumers: 3)
        .AddRabbitConsumer<BarQueueHandler>("BarConsumer", consumers: 2);

        builder.Services.AddHostedService<PublishService>();

        using IHost host = builder.Build();

        host.Run();

        var rabbitMqHelper = host.Services.GetRequiredService<IRabbitHelper>();

        rabbitMqHelper.Publish("FooProducer", "Hello, World!");

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    public class FooQueueHandler : DefaultMessageHandler
    {
        public override void HandleMessage(string message)
        {
            Console.WriteLine($"[x] Received from foo-queue: {message}");
            Task.Delay(1000).Wait();
            Console.WriteLine("[x] Done");
        }
    }

    public class BarQueueHandler : DefaultMessageHandler
    {
        public override void HandleMessage(string message)
        {
            Console.WriteLine($"[x] Received from bar-queue: {message}");
            Task.Delay(500).Wait();
            Console.WriteLine("[x] Done");
        }
    }
}
