using Example.MultiConsumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitConsumer(options =>
{
    options.AddConsumer(new ConsumerOptions
    {
        ConsumerName = "FooFirstQueueConsumer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        QueueName = "FooFirstQueue",
        AutomaticRecoveryEnabled = true
    });
    options.AddConsumer(new ConsumerOptions
    {
        ConsumerName = "FooSecondQueueConsumer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        QueueName = "FooSecondQueue",
        AutomaticRecoveryEnabled = true
    });
});

builder.Logging.AddConsole();
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Program init");

// register BackgroundService
builder.Services.AddRabbitSubscriber<FirstConsumeService>("FooFirstQueueConsumer", enableLogging: false);
builder.Services.AddRabbitAsyncSubscriber<SecondConsumeService>("FooSecondQueueConsumer", enableLogging: false);

using IHost host = builder.Build();

await host.RunAsync();