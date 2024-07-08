using Autofac;
using Autofac.Extensions.DependencyInjection;
using Example.NLog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;
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

        services.AddRabbitHelper(builder =>
        {
            builder.SetHostName("localhost");
            builder.SetPort(5672);
            builder.SetVirtualHost("/");
            builder.SetUserName("admin");
            builder.SetPassword("admin");
            builder.EnableLogging(false);
            builder.AddProducerOption(producer =>
            {
                producer.ProducerName = "FooProducer";
                producer.ExchangeName = "amq.topic";
                producer.RoutingKey = "foo.key";
                producer.Type = ExchangeType.Topic;
            });
            builder.AddProducerOption(producer =>
            {
                producer.ProducerName = "BarProducer";
                producer.ExchangeName = "amq.direct";
                producer.RoutingKey = "bar.key";
                producer.Type = ExchangeType.Direct;
            });
            builder.AddConsumerOption(consumer =>
            {
                consumer.ConsumerName = "FooConsumer";
                consumer.QueueName = "foo-queue";
            });
            builder.AddConsumerOption(consumer =>
            {
                consumer.ConsumerName = "BarConsumer";
                consumer.QueueName = "bar-queue";
            });
        })
        .AddRabbitConsumer<FooQueueHandler>("FooConsumer", consumers: 3)
        .AddRabbitConsumer<BarQueueHandler>("BarConsumer", consumers: 2);

        // register BackgroundService
        services.AddHostedService<PublishService>();
    });

public class FooQueueHandler : DefaultMessageHandler
{
    private readonly ILogger<FooQueueHandler> _logger;

    public FooQueueHandler(ILogger<FooQueueHandler> logger)
    {
        _logger = logger;
    }

    public override void HandleMessage(string message)
    {
        _logger.LogInformation($"[x] Received from foo-queue: {message}");
        Task.Delay(1000).Wait();
        _logger.LogInformation("[x] Done");
    }
}

public class BarQueueHandler : DefaultMessageHandler
{
    private readonly ILogger<BarQueueHandler> _logger;

    public BarQueueHandler(ILogger<BarQueueHandler> logger)
    {
        _logger = logger;
    }

    public override void HandleMessage(string message)
    {
        _logger.LogInformation($"[x] Received from bar-queue: {message}");
        Task.Delay(500).Wait();
        _logger.LogInformation("[x] Done");
    }
}