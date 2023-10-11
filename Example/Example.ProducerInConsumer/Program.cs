using Example.ProducerInConsumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;
using NanoRabbit.DependencyInjection;
using NanoRabbit.Producer;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddScoped<RabbitProducer>(_ =>
{
    var producer = new RabbitProducer(new[]
    {
        new ProducerOptions
        {
            ProducerName = "FooSecondQueueProducer",
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin",
            VirtualHost = "FooHost",
            ExchangeName = "amq.topic",
            RoutingKey = "FooSecondKey",
            Type = ExchangeType.Topic,
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
builder.Services.AddHostedService<ConsumeService>();

using var host = builder.Build();

await host.RunAsync();