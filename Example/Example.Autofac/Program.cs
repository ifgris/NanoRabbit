using NanoRabbit.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Example.Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;

try
{
    var host = CreateHostBuilder(args).Build();
    await host.RunAsync();
}
catch (Exception)
{
    throw;
}

IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>((context, builders) =>
    {
        // ...
    })
    .ConfigureServices((context, services) =>
    {
        services.AddRabbitPool(config =>
        {
            config.EnableLogging = true;
        }, list =>
        {
            list.Add(new ConnectOptions("Connection1", options =>
            {
                options.ConnectConfig = new ConnectConfig(c =>
                {
                    c.HostName = "localhost";
                    c.Port = 5672;
                    c.UserName = "admin";
                    c.Password = "admin";
                    c.VirtualHost = "FooHost";
                });
                options.ProducerConfigs = new List<ProducerConfig>
                {
                    new ProducerConfig("FooFirstQueueProducer", c =>
                    {
                        c.ExchangeName = "FooTopic";
                        c.RoutingKey = "FooFirstKey";
                        c.Type = ExchangeType.Topic;
                    })
                };
                options.ConsumerConfigs = new List<ConsumerConfig>
                {
                    new ConsumerConfig("FooFirstQueueConsumer", c =>
                    {
                        c.QueueName = "FooFirstQueue";
                    })
                };
            }));
            list.Add(new ConnectOptions("Connection2", options =>
            {
                options.ConnectConfig = new ConnectConfig(c =>
                {
                    c.HostName = "localhost";
                    c.Port = 5672;
                    c.UserName = "admin";
                    c.Password = "admin";
                    c.VirtualHost = "BarHost";
                });
                options.ProducerConfigs = new List<ProducerConfig>
                {
                    new ProducerConfig("BarFirstQueueProducer", c =>
                    {
                        c.ExchangeName = "BarDirect";
                        c.RoutingKey = "BarFirstKey";
                        c.Type = ExchangeType.Direct;
                    })
                };
                options.ConsumerConfigs = new List<ConsumerConfig>
                {
                    new ConsumerConfig("BarFirstQueueConsumer", c => { c.QueueName = "BarFirstQueue"; })
                };
            }));
        });

        // register the customize RabbitProducer
        services.AddProducer<FooFirstQueueProducer>("Connection1", "FooFirstQueueProducer");
        services.AddProducer<BarFirstQueueProducer>("Connection2", "BarFirstQueueProducer");
        
        // register the customize RabbitConsumer
        services.AddConsumer<FooFirstQueueConsumer, string>("Connection1", "FooFirstQueueConsumer");
        
        // register BackgroundService
        services.AddHostedService<PublishService>();
        services.AddHostedService<ConsumeService>();
    });