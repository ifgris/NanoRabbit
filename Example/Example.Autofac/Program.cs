using System.Text;
using NanoRabbit.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Example.Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit;

var host = CreateHostBuilder(args).Build();
await host.RunAsync();

IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>((context, builders) =>
    {
        // ...
    })
    .ConfigureServices((context, services) =>
    {
        services.AddRabbitHelper(builder =>
            {
                builder.SetHostName("localhost")
                    .SetPort(5672)
                    .SetVirtualHost("/")
                    .SetUserName("admin")
                    .SetPassword("admin")
                    .AddProducerOption(producer =>
                    {
                        producer.ProducerName = "FooProducer";
                        producer.ExchangeName = "amq.topic";
                        producer.RoutingKey = "foo.key";
                        producer.Type = ExchangeType.Topic;
                    })
                    .AddProducerOption(producer =>
                    {
                        producer.ProducerName = "BarProducer";
                        producer.ExchangeName = "amq.direct";
                        producer.RoutingKey = "bar.key";
                        producer.Type = ExchangeType.Direct;
                    })
                    .AddConsumerOption(consumer =>
                    {
                        consumer.ConsumerName = "FooConsumer";
                        consumer.QueueName = "foo-queue";
                        consumer.ConsumerCount = 3;
                        consumer.HandlerName = nameof(FooQueueHandler);
                    })
                    .AddConsumerOption(consumer =>
                    {
                        consumer.ConsumerName = "BarConsumer";
                        consumer.QueueName = "bar-queue";
                        consumer.ConsumerCount = 2;
                        consumer.HandlerName = nameof(BarQueueHandler);
                    });
            }, serviceCollection =>
            {
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });

                var logger = loggerFactory.CreateLogger("RabbitHelper");

                return logger;
            })
            .AddRabbitAsyncHandler<FooQueueHandler>()
            .AddRabbitAsyncHandler<BarQueueHandler>()
            .AddRabbitConsumerService();

        // register BackgroundService
        services.AddHostedService<PublishService>();
    });

public class FooQueueHandler : IAsyncMessageHandler
{
    public async Task HandleMessageAsync(byte[] messageBody, string? routingKey = null, string? correlationId = null)
    {
        var message = Encoding.UTF8.GetString(messageBody);
        Console.WriteLine($"[x] Received from foo-queue: {message}");
        await Task.Delay(1000);
        Console.WriteLine("[x] Done");
    }
}

public class BarQueueHandler : IAsyncMessageHandler
{
    public async Task HandleMessageAsync(byte[] messageBody, string? routingKey = null, string? correlationId = null)
    {
        var message = Encoding.UTF8.GetString(messageBody);
        Console.WriteLine($"[x] Received from bar-queue: {message}");
        await Task.Delay(500);
        Console.WriteLine("[x] Done");
    }
}