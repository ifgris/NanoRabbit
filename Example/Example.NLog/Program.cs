using Autofac;
using Autofac.Extensions.DependencyInjection;
using Example.NLog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;
using NanoRabbit.Producer;
using NLog;
using NLog.Extensions.Logging;

var logger = LogManager.GetCurrentClassLogger();
try
{
    logger.Info("Init Program");
    var host = CreateHostBuilder(args).Build();
    await host.RunAsync();
}
catch (Exception e)
{
    logger.Error(e, e.Message);
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
        services.AddLogging(loggingBuilder =>
        {
            // configure Logging with NLog
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            loggingBuilder.AddNLog(context.Configuration);
        }).BuildServiceProvider();

        services.AddScoped<RabbitProducer>(_ =>
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

        services.AddScoped<RabbitConsumer>(_ =>
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

        // register BackgroundService
        services.AddHostedService<PublishService>();
        services.AddHostedService<ConsumeService>();
    });