using Example.PublishToQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
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
                producer.RoutingKey = "no-key-queue";
            });
    })
    .AddRabbitHelper();


builder.Services.AddHostedService<PublishService>();

using IHost host = builder.Build();

host.Run();

var rabbitMqHelper = host.Services.GetRequiredService<IRabbitHelper>();

await rabbitMqHelper.PublishAsync("FooProducer", "Hello, World!");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
