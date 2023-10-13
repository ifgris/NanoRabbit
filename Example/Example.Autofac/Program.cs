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
        services.AddRabbitProducer(options =>
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
            });
        });

        services.AddRabbitConsumer(options =>
        {
            options.AddConsumer(new ConsumerOptions
            {
                ConsumerName = "FooFirstQueueConsumer",
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "admin",
                VirtualHost = "FooHost",
                QueueName = "FooFirstQueue"
            });
            options.AddConsumer(new ConsumerOptions
            {
                ConsumerName = "BarFirstQueueConsumer",
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "admin",
                VirtualHost = "BarHost",
                QueueName = "BarFirstQueue"
            });
        });

        // register BackgroundService
        services.AddHostedService<PublishService>();
        services.AddHostedService<ConsumeService>();
    });