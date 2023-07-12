using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.NanoRabbit;
using RabbitMQ.Client;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitPool(new Dictionary<string, ConnectOptions>
    {
        {"Connection1", new ConnectOptions
            {
                ConnectConfig = new ConnectConfig
                    {
                        HostName = "localhost",
                        Port = 5672,
                        UserName = "admin",
                        Password = "admin",
                        VirtualHost = "DATA"
                    },
                ProducerConfigs = new Dictionary<string, ProducerConfig>
                    {
                        {"DataBasicQueueProducer", new ProducerConfig
                        {
                            ExchangeName = "BASIC.TOPIC",
                            RoutingKey = "BASIC.KEY",
                            Type = ExchangeType.Topic
                        }
                        }
                    }
        }
        },
        {"Connection2", new ConnectOptions
            {
                ConnectConfig = new ConnectConfig
                    {
                        HostName = "localhost",
                        Port = 25672,
                        UserName = "admin",
                        Password = "admin",
                        VirtualHost = "HOST"
                    },
                ProducerConfigs = new Dictionary<string, ProducerConfig>
                    {
                        {"HostBasicQueueProducer", new ProducerConfig
                        {
                            ExchangeName = "BASIC.TOPIC",
                            RoutingKey = "BASIC.KEY",
                            Type = ExchangeType.Topic
                        }
                        }
                    }
        }
        }
    });

builder.Services.AddHostedService<PublishService>();
using IHost host = builder.Build();

await host.RunAsync();
