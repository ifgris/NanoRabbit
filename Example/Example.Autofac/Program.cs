using NanoRabbit.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Example.Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
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
        services.AddRabbitHelper(builder =>
        {
            builder.SetHostName("localhost");
            builder.SetPort(5672);
            builder.SetVirtualHost("/");
            builder.SetUserName("admin");
            builder.SetPassword("admin");
            builder.AddProducer(producer =>
            {
                producer.ProducerName = "FooProducer";
                producer.ExchangeName = "amq.topic";
                producer.RoutingKey = "foo.key";
                producer.Type = ExchangeType.Topic;
            });
            builder.AddProducer(producer =>
            {
                producer.ProducerName = "BarProducer";
                producer.ExchangeName = "amq.direct";
                producer.RoutingKey = "bar.key";
                producer.Type = ExchangeType.Direct;
            });
            builder.AddConsumer(consumer =>
            {
                consumer.ConsumerName = "FooConsumer";
                consumer.QueueName = "foo-queue";
            });
            builder.AddConsumer(consumer =>
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
    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from foo-queue: {message}");
        Task.Delay(1000).Wait();
        Console.WriteLine("[x] Done");
    }
}

public class BarQueueHandler : DefaultMessageHandler
{
    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from bar-queue: {message}");
        Task.Delay(500).Wait();
        Console.WriteLine("[x] Done");
    }
}