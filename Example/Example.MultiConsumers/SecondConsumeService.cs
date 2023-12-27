using Microsoft.Extensions.Hosting;
using NanoRabbit.Consumer;

namespace Example.MultiConsumers;

public class SecondConsumeService : BackgroundService
{
    private readonly IRabbitConsumer _consumer;

    public SecondConsumeService(IRabbitConsumer consumer)
    {
        _consumer = consumer;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("FooSecondQueueConsumer running...");
        while (!stoppingToken.IsCancellationRequested)
        {
            await _consumer.Receive("FooSecondQueueConsumer", message => { Console.WriteLine(message); });
            await Task.Delay(1000, stoppingToken);
        }
    }
}