using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Helper;

namespace Example.MultiConnections;

public class DefaultPublishService : BackgroundService
{
    private readonly ILogger<DefaultPublishService> _logger;
    private readonly IRabbitHelper _rabbitHelper;

    public DefaultPublishService(ILogger<DefaultPublishService> logger, [FromKeyedServices("DefaultRabbitHelper")]IRabbitHelper rabbitHelper)
    {
        _logger = logger;
        _rabbitHelper = rabbitHelper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Testing DefaultPublishService");

        while (!stoppingToken.IsCancellationRequested)
        {
            _rabbitHelper.Publish("FooProducer", "Hello from default publish service.");
            await Task.Delay(1000, stoppingToken);
        }
    }
}