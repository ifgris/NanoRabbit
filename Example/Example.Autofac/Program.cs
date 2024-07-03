using NanoRabbit.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Example.Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;
using NanoRabbit.Helper.MessageHandler;

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
            builder.AddProducer(new ProducerOptions
            {
                ProducerName = "FooProducer",
                ExchangeName = "amq.topic",
                RoutingKey = "foo.key",
                Type = ExchangeType.Topic
            });
            builder.AddProducer(new ProducerOptions
            {
                ProducerName = "BarProducer",
                ExchangeName = "amq.direct",
                RoutingKey = "bar.key",
                Type = ExchangeType.Direct
            });
            builder.AddConsumer(new ConsumerOptions
            {
                ConsumerName = "FooConsumer",
                QueueName = "foo-queue"
            });
            builder.AddConsumer(new ConsumerOptions
            {
                ConsumerName = "BarConsumer",
                QueueName = "bar-queue"
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