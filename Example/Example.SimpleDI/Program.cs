using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;
using NanoRabbit.Helper;
using NanoRabbit.Helper.MessageHandler;

class Program
{
    static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var rabbitMqHelper = host.Services.GetRequiredService<IRabbitHelper>();

        rabbitMqHelper.Publish("FooProducer", "Hello, World!");

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddRabbitHelper(builder =>
                {
                    builder.SetHostName("localhost");
                    builder.SetPort(5672);
                    builder.SetVirtualHost("/");
                    builder.SetUserName("admin");
                    builder.SetPassword("admin");
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
            });

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
}
