using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit;

namespace Example.Autofac;

public class PublishService : BackgroundService
{
    private readonly ILogger<PublishService> _logger;
    private readonly IRabbitHelper _rabbitHelper;

    public PublishService(ILogger<PublishService> logger, IRabbitHelper rabbitHelper)
    {
        _logger = logger;
        _rabbitHelper = rabbitHelper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Testing PublishService");

        while (!stoppingToken.IsCancellationRequested)
        {
            _rabbitHelper.Publish("FooProducer", "Hello from conn1");
            _rabbitHelper.Publish("BarProducer", "Hello from conn2");
            await Task.Delay(1000, stoppingToken);
        }
    }
}