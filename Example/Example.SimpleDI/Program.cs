using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;
using NanoRabbit.Helper;
using NanoRabbit.Helper.MessageHandler;

//var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

//var builder = Host.CreateApplicationBuilder(args);

//builder.Services.AddRabbitProducer(options =>
//{
//    options.AddProducer(new ProducerOptions
//    {
//        ProducerName = "FooFirstQueueProducer",
//        HostName = "localhost",
//        Port = 5672,
//        UserName = "admin",
//        Password = "admin",
//        VirtualHost = "FooHost",
//        ExchangeName = "amq.topic",
//        RoutingKey = "FooFirstKey",
//        Type = ExchangeType.Topic,
//        Durable = true,
//        AutoDelete = false,
//        Arguments = null,
//        AutomaticRecoveryEnabled = true
//    });
//    options.AddProducer(new ProducerOptions
//    {
//        ProducerName = "BarFirstQueueProducer",
//        HostName = "localhost",
//        Port = 5672,
//        UserName = "admin",
//        Password = "admin",
//        VirtualHost = "BarHost",
//        ExchangeName = "amq.direct",
//        RoutingKey = "BarFirstKey",
//        Type = ExchangeType.Direct,
//        Durable = true,
//        AutoDelete = false,
//        Arguments = null,
//        AutomaticRecoveryEnabled = true
//    });
//});

//builder.Services.AddRabbitConsumer(options =>
//{
//    options.AddConsumer(new ConsumerOptions
//    {
//        ConsumerName = "FooFirstQueueConsumer",
//        HostName = "localhost",
//        Port = 5672,
//        UserName = "admin",
//        Password = "admin",
//        VirtualHost = "FooHost",
//        QueueName = "FooFirstQueue",
//        AutomaticRecoveryEnabled = true,
//        PrefetchCount = 500,
//        PrefetchSize = 0
//    });
//    options.AddConsumer(new ConsumerOptions
//    {
//        ConsumerName = "BarFirstQueueConsumer",
//        HostName = "localhost",
//        Port = 5672,
//        UserName = "admin",
//        Password = "admin",
//        VirtualHost = "BarHost",
//        QueueName = "BarFirstQueue", 
//        AutomaticRecoveryEnabled = true
//    });
//});

//builder.Logging.AddConsole();
//var logger = loggerFactory.CreateLogger<Program>();
//logger.LogInformation("Program init");

//// register BackgroundService
//builder.Services.AddHostedService<PublishService>();
//builder.Services.AddRabbitSubscriber<ConsumeService>("FooFirstQueueConsumer");

//using IHost host = builder.Build();

//await host.RunAsync();

class Program
{
    static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var rabbitMqHelper = host.Services.GetRequiredService<IRabbitHelper>();

        // 示例使用
        rabbitMqHelper.Publish("data.topic", "foo.key", "Hello, World!");

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddRabbitMqHelper("localhost", userName:"admin", password:"admin",virtualHost: "foo")
                        .AddRabbitConsumer<TaskQueueHandler>("foo-queue", consumers: 3) // 添加3个消费者
                        .AddRabbitConsumer<AnotherQueueHandler>("another_queue", consumers: 2); // 添加2个消费者
            });

    public class TaskQueueHandler : DefaultMessageHandler
    {
        public override void HandleMessage(string message)
        {
            Console.WriteLine($"[x] Received from task_queue: {message}");
            // 自定义处理逻辑
            Task.Delay(1000).Wait();
            Console.WriteLine("[x] Done");
        }
    }

    public class AnotherQueueHandler : DefaultMessageHandler
    {
        public override void HandleMessage(string message)
        {
            Console.WriteLine($"[x] Received from another_queue: {message}");
            // 自定义处理逻辑
            Task.Delay(500).Wait();
            Console.WriteLine("[x] Done");
        }
    }

}
