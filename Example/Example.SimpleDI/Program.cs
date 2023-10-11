using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;
using NanoRabbit.DependencyInjection;
using NanoRabbit.Producer;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<RabbitProducer>(_ =>
{
    var producer = new RabbitProducer(new[]
    {
        new ProducerOptions
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
        },
        new ProducerOptions
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
        }
    });
    return producer;
});

builder.Services.AddScoped<RabbitConsumer>(_ =>
{
    var consumer = new RabbitConsumer(new[]
    {
        new ConsumerOptions
        {
            ConsumerName = "FooFirstQueueConsumer",
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin",
            VirtualHost = "FooHost",
            QueueName = "FooFirstQueue"
        },
        new ConsumerOptions
        {
            ConsumerName = "BarFirstQueueConsumer",
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin",
            VirtualHost = "BarHost",
            QueueName = "BarFirstQueue"
        }
    });
    return consumer;
});

builder.Logging.AddConsole();
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Program init");

// register BackgroundService
builder.Services.AddHostedService<PublishService>();
builder.Services.AddHostedService<ConsumeService>();

using IHost host = builder.Build();

await host.RunAsync();