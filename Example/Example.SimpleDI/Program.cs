using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddRabbitPool(
    globalConfig =>
    {
        globalConfig.EnableLogging = true;
    }, 
    c =>
    {
        c.Add(new ConnectOptions("Connection1", option =>
        {
            option.ConnectConfig = new(config =>
            {
                config.HostName = "localhost";
                config.Port = 5672;
                config.UserName = "admin";
                config.Password = "admin";
                config.VirtualHost = "FooHost";
            });
            option.ProducerConfigs = new List<ProducerConfig>
            {
                new ProducerConfig("FooFirstQueueProducer", c =>
                {
                    c.ExchangeName = "FooTopic";
                    c.RoutingKey = "FooFirstKey";
                    c.Type = ExchangeType.Topic;
                })
            };
            option.ConsumerConfigs = new List<ConsumerConfig>
            {
                new ConsumerConfig("FooFirstQueueConsumer", c =>
                {
                    c.QueueName = "FooFirstQueue";
                })
            };
        }));

        c.Add(new ConnectOptions("Connection2", option =>
        {
            option.ConnectConfig = new(config =>
            {
                config.HostName = "localhost";
                config.Port = 5672;
                config.UserName = "admin";
                config.Password = "admin";
                config.VirtualHost = "BarHost";
            });
            option.ProducerConfigs = new List<ProducerConfig>
            {
                new ProducerConfig("BarFirstQueueProducer", c =>
                {
                    c.ExchangeName = "BarDirect";
                    c.RoutingKey = "BarFirstKey";
                    c.Type = ExchangeType.Direct;
                })
            };
            option.ConsumerConfigs = new List<ConsumerConfig>
            {
                new ConsumerConfig("BarFirstQueueConsumer", c =>
                {
                    c.QueueName = "BarFirstQueue";
                })
            };
        }));
    });

builder.Logging.AddConsole();
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Program init");

// register the customize RabbitProducer
builder.Services.AddProducer<FooFirstQueueProducer>("Connection1", "FooFirstQueueProducer");
builder.Services.AddProducer<BarFirstQueueProducer>("Connection2", "BarFirstQueueProducer");

// register the customize RabbitConsumer
builder.Services.AddConsumer<FooFirstQueueConsumer, string>("Connection1", "FooFirstQueueConsumer");

// register BackgroundService
builder.Services.AddHostedService<PublishService>();
// builder.Services.AddHostedService<ConsumeService>();

using IHost host = builder.Build();

await host.RunAsync();
