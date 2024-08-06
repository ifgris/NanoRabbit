using Example.ReadSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitHelperFromAppSettings<FooConfiguration>(builder.Configuration)
    .AddRabbitConsumer<FooQueueHandler>("FooConsumer")
    .AddRabbitConsumer<BarQueueHandler>("BarConsumer");

builder.Services.AddHostedService<PublishService>();

var host = builder.Build();
await host.RunAsync();

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

public class FooConfiguration : RabbitConfiguration { }