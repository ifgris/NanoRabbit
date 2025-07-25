using System.Text;
using Example.ReadSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitHelperFromAppSettings<FooConfiguration>(builder.Configuration)
    .AddRabbitAsyncHandler<FooQueueHandler>()
    .AddRabbitAsyncHandler<BarQueueHandler>()
    .AddRabbitConnection(builder.Configuration)
    .AddRabbitConsumerFromAppSettings<FooConfiguration>(builder.Configuration);

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

public class FooConfiguration : RabbitConfiguration
{
}