using NanoRabbit.DependencyInjection;
using Example.ReadSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.Helper.MessageHandler;
using NanoRabbit.Connection;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddRabbitMqHelperFromAppSettings<FooConfiguration>(builder.Configuration)
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
        // 自定义处理逻辑
        Task.Delay(1000).Wait();
        Console.WriteLine("[x] Done");
    }
}

public class BarQueueHandler : DefaultMessageHandler
{
    public override void HandleMessage(string message)
    {
        Console.WriteLine($"[x] Received from bar-queue: {message}");
        // 自定义处理逻辑
        Task.Delay(500).Wait();
        Console.WriteLine("[x] Done");
    }
}

public class FooConfiguration : RabbitConfiguration { }