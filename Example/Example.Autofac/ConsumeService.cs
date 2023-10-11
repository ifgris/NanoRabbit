using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace Example.Autofac;

public class ConsumeService : BackgroundService
{
    private readonly RabbitConsumer _consumer;

    public ConsumeService(RabbitConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _consumer.Receive("FooFirstQueueConsumer", message => { Console.WriteLine(message); });
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
}