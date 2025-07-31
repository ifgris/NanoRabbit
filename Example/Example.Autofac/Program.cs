using System.Text;
using NanoRabbit.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Example.Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureContainer(new AutofacServiceProviderFactory(), _ =>
{
    
});

builder.Services.AddRabbitConnection(x =>
    {
        x.SetHostName("localhost")
            .SetPort(5672)
            .SetVirtualHost("/")
            .SetUserName("admin")
            .SetPassword("admin")
            .SetConnectionName("FooConnection")
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
    })
    .AddRabbitHelper()
    .AddRabbitAsyncHandler<FooQueueHandler>()
    .AddRabbitAsyncHandler<BarQueueHandler>()
            
    .AddRabbitConsumer();

builder.Services.AddHostedService<PublishService>();

var host = builder.Build();

await host.RunAsync();

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