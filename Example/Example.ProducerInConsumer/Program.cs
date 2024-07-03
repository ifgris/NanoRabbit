using Example.ProducerInConsumer;
using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddRabbitHelper(builder =>
{
    builder.SetHostName("localhost");
    builder.SetPort(5672);
    builder.SetVirtualHost("/");
    builder.SetUserName("admin");
    builder.SetPassword("admin");
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
})
.AddRabbitConsumer<FooQueueHandler>("FooConsumer", consumers: 3);

using IHost host = builder.Build();

await host.RunAsync();