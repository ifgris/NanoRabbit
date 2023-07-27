using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddRabbitPool(c =>
{
    c.Add(new ConnectOptions("Connection1", option =>
    {
        option.ConnectConfig = new(config =>
        {
            config.HostName = "localhost";
            config.Port = 5672;
            config.UserName = "admin";
            config.Password = "admin";
            config.VirtualHost = "DATA";
        });
        option.ProducerConfigs = new List<ProducerConfig>
        {
            new ProducerConfig("DataBasicQueueProducer", c =>
            {
                c.ExchangeName = "BASIC.TOPIC";
                c.RoutingKey = "BASIC.KEY";
                c.Type = ExchangeType.Topic;
            })
        };
        option.ConsumerConfigs = new List<ConsumerConfig>
        {
            new ConsumerConfig("DataBasicQueueConsumer", c =>
            {
                c.QueueName = "BASIC_QUEUE";
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
            config.VirtualHost = "HOST";
        });
        option.ProducerConfigs = new List<ProducerConfig>
        {
            new ProducerConfig("HostBasicQueueProducer", c =>
            {
                c.ExchangeName = "BASIC.TOPIC";
                c.RoutingKey = "BASIC.KEY";
                c.Type = ExchangeType.Topic;
            })
        };
        option.ConsumerConfigs = new List<ConsumerConfig>
        {
            new ConsumerConfig("HostBasicQueueConsumer", c =>
            {
                c.QueueName = "BASIC_QUEUE";
            })
        };
    }));
});

// register the customize RabbitProducer
builder.Services.AddProducer<DataBasicQueueProducer>("Connection1", "DataBasicQueueProducer");
builder.Services.AddProducer<HostBasicQueueProducer>("Connection2", "HostBasicQueueProducer");

builder.Services.AddConsumer<BasicConsumer, string>("Connection1", "DataBasicQueueConsumer");

// register BackgroundService
builder.Services.AddHostedService<PublishService>();
builder.Services.AddHostedService<ConsumeService>();

using IHost host = builder.Build();

await host.RunAsync();
