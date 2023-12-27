using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Producer;

namespace Example.Autofac;

public class PublishService : BackgroundService
{
    private readonly ILogger<PublishService> _logger;
    private readonly IRabbitProducer _producer;

    public PublishService(ILogger<PublishService> logger, IRabbitProducer producer)
    {
        _logger = logger;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Testing PublishService");

        while (!stoppingToken.IsCancellationRequested)
        {
            _producer.Publish("FooFirstQueueProducer", "Hello from conn1");
            _producer.Publish("BarFirstQueueProducer", "Hello from conn2");
            await Task.Delay(1000, stoppingToken);
        }
    }
}