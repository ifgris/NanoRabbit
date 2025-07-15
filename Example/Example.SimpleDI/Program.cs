using System.Text;
using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddRabbitHelper(builder =>
{
    builder.SetHostName("localhost")
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
.AddRabbitAsyncHandler<FooQueueHandler>()
.AddRabbitAsyncHandler<BarQueueHandler>()
.AddRabbitConsumerService();

builder.Services.AddHostedService<PublishService>();

using IHost host = builder.Build();

host.Run();

var rabbitMqHelper = host.Services.GetRequiredService<IRabbitHelper>();

await rabbitMqHelper.PublishAsync("FooProducer", "Hello, World!");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();


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