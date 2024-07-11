using Example.MultiConnections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddKeyedRabbitHelper("DefaultRabbitHelper", builder =>
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
        .AddConsumerOption(consumer =>
        {
            consumer.ConsumerName = "DefaultFooConsumer";
            consumer.QueueName = "foo-queue";
        });
}).AddKeyedRabbitConsumer<DefaultMessageHandler>("DefaultRabbitHelper", "DefaultFooConsumer");

builder.Services.AddKeyedRabbitHelper("TestRabbitHelper", builder =>
{
    builder.SetHostName("localhost")
    .SetPort(5672)
    .SetVirtualHost("test")
    .SetUserName("admin")
    .SetPassword("admin")
    .AddProducerOption(producer =>
    {
        producer.ProducerName = "FooProducer";
        producer.ExchangeName = "amq.topic";
        producer.RoutingKey = "foo.key";
        producer.Type = ExchangeType.Topic;
    })
    .AddConsumerOption(consumer =>
    {
        consumer.ConsumerName = "TestFooConsumer";
        consumer.QueueName = "foo-queue";
    });
}).AddKeyedRabbitConsumer<DefaultMessageHandler>("TestRabbitHelper", "TestFooConsumer");

builder.Services.AddHostedService<DefaultPublishService>();
builder.Services.AddHostedService<TestPublishService>();

var host = builder.Build();
await host.RunAsync();

public class DefaultFooQueueHandler : DefaultMessageHandler
{
    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from default foo-queue: {message}");
        // 自定义处理逻辑
        Task.Delay(1000).Wait();
        Console.WriteLine("[x] Done");
    }
}

public class TestFooQueueHandler : DefaultMessageHandler
{
    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from test foo-queue: {message}");
        // 自定义处理逻辑
        Task.Delay(1000).Wait();
        Console.WriteLine("[x] Done");
    }
}