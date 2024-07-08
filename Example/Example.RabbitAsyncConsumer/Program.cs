using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddRabbitHelper(builder =>
{
    builder.SetHostName("localhost");
    builder.SetPort(5672);
    builder.SetVirtualHost("/");
    builder.SetUserName("admin");
    builder.SetPassword("admin");
    builder.UseAsyncConsumer(true); // set UseAsyncConsumer to true
    builder.AddProducerOption(producer =>
    {
        producer.ProducerName = "FooProducer";
        producer.ExchangeName = "amq.topic";
        producer.RoutingKey = "foo.key";
        producer.Type = ExchangeType.Topic;
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
.AddAsyncRabbitConsumer<FooQueueHandler>("FooConsumer", consumers: 3)
.AddAsyncRabbitConsumer<BarQueueHandler>("BarConsumer", consumers: 2);

var host = builder.Build();
await host.RunAsync();

public class FooQueueHandler : DefaultAsyncMessageHandler
{
    public override async Task HandleMessageAsync(string message)
    {
        Console.WriteLine($"[x] Received from foo-queue: {message}");
        await Task.Delay(1000);
        Console.WriteLine("[x] Done");
    }
}

public class BarQueueHandler : DefaultAsyncMessageHandler
{
    public override async Task HandleMessageAsync(string message)
    {
        Console.WriteLine($"[x] Received from bar-queue: {message}");
        await Task.Delay(500);
        Console.WriteLine("[x] Done");
    }
}
