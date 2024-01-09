using Example.ProducerInConsumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddRabbitProducer(options =>
{
    options.AddProducer(new ProducerOptions
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
        AutomaticRecoveryEnabled = true
    });
});

builder.Services.AddRabbitConsumer(options =>
{
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
builder.Services.AddRabbitSubscriber<ConsumeService>("BarFirstQueueConsumer");

using IHost host = builder.Build();

await host.RunAsync();