using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.NLog;

public class ConsumeService : BackgroundService
{
    private readonly ILogger<ConsumeService> _logger;
    private readonly RabbitConsumer _consumer;

    public ConsumeService(RabbitConsumer consumer, ILogger<ConsumeService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _consumer.Receive("FooFirstQueueConsumer", message => { _logger.LogInformation(message); });
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
}