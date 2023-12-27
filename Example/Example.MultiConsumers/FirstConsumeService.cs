using Microsoft.Extensions.Hosting;
using NanoRabbit.Consumer;

namespace Example.MultiConsumers;

public class FirstConsumeService : BackgroundService
{
    private readonly IRabbitConsumer _consumer;

    public FirstConsumeService(IRabbitConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("FooFirstQueueConsumer running...");
        while (!stoppingToken.IsCancellationRequested)
        {
            await _consumer.Receive("FooFirstQueueConsumer", message => { Console.WriteLine(message); });
            await Task.Delay(1000, stoppingToken);
        }
    }
}