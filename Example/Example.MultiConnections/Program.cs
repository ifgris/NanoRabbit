using Microsoft.Extensions.Hosting;
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
});

var host = builder.Build();
await host.RunAsync();
