using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.ReadSettings;

public class ConsumeService : BackgroundService
{
    private readonly ILogger<ConsumeService> _logger;
    private readonly IRabbitConsumer _consumer;

    public ConsumeService(IRabbitConsumer consumer, ILogger<ConsumeService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _consumer.ReceiveAsync("FooFirstQueueConsumer", HandleMessage,
                prefetchCount: 500);
        }

        await Task.Delay(1000, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }

    private void HandleMessage(string message)
    {
        // Console.WriteLine(message);
        _logger.LogInformation(message);
    }
}