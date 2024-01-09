using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitProducer(options =>
{
    options.AddProducer(new ProducerOptions
    {
        ProducerName = "FooFirstQueueProducer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        ExchangeName = "amq.topic",
        RoutingKey = "FooFirstKey",
        Type = ExchangeType.Topic,
        Durable = true,
        AutoDelete = false,
        Arguments = null,
        AutomaticRecoveryEnabled = true
    });
    options.AddProducer(new ProducerOptions
    {
        ProducerName = "BarFirstQueueProducer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "BarHost",
        ExchangeName = "amq.direct",
        RoutingKey = "BarFirstKey",
        Type = ExchangeType.Direct,
        Durable = true,
        AutoDelete = false,
        Arguments = null,
        AutomaticRecoveryEnabled = true
    });
});

builder.Services.AddRabbitConsumer(options =>
{
    options.AddConsumer(new ConsumerOptions
    {
        ConsumerName = "FooFirstQueueConsumer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        QueueName = "FooFirstQueue",
        AutomaticRecoveryEnabled = true,
        PrefetchCount = 500,
        PrefetchSize = 0
    });
    options.AddConsumer(new ConsumerOptions
    {
        ConsumerName = "BarFirstQueueConsumer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "BarHost",
        QueueName = "BarFirstQueue", 
        AutomaticRecoveryEnabled = true
    });
});

builder.Logging.AddConsole();
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Program init");

// register BackgroundService
builder.Services.AddHostedService<PublishService>();
builder.Services.AddRabbitSubscriber<ConsumeService>("FooFirstQueueConsumer");

using IHost host = builder.Build();

await host.RunAsync();