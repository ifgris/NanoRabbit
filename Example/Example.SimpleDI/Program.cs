using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;
using RabbitMQ.Client;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitPool(c =>
{
    c.Add(new ConnectOptions
    {
        ConnectionName = "Connection1",
        ConnectConfig = new ConnectConfig
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin",
            VirtualHost = "DATA"
        },
        ProducerConfigs = new List<ProducerConfig> { 
            new ProducerConfig
            {
                ProducerName = "DataBasicQueueProducer",
                ExchangeName = "BASIC.TOPIC",
                RoutingKey = "BASIC.KEY",
                Type = ExchangeType.Topic
            }
        },
        ConsumerConfigs = new List<ConsumerConfig>
        {
            new ConsumerConfig
            {
                ConsumerName = "DataBasicQueueConsumer",
                QueueName = "BASIC_QUEUE"
            }
        }
    });

    c.Add(new ConnectOptions
    {
        ConnectionName = "Connection2",
        ConnectConfig = new ConnectConfig
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin",
            VirtualHost = "HOST"
        },
        ProducerConfigs = new List<ProducerConfig> {
            new ProducerConfig
            {
                ProducerName = "HostBasicQueueProducer",
                ExchangeName = "BASIC.TOPIC",
                RoutingKey = "BASIC.KEY",
                Type = ExchangeType.Topic
            }
        },
        ConsumerConfigs = new List<ConsumerConfig>
        {
            new ConsumerConfig
            {
                ConsumerName = "HostBasicQueueConsumer",
                QueueName = "BASIC_QUEUE"
            }
        }
    });
});

builder.Services.AddProducer<DataBasicQueueProducer>("Connection1", "DataBasicQueueProducer");
builder.Services.AddProducer<HostBasicQueueProducer>("Connection2", "DataBasicQueueProducer");

builder.Services.AddHostedService<PublishService>();
//builder.Services.AddHostedService<ConsumeService>();
using IHost host = builder.Build();

await host.RunAsync();
