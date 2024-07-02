using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Helper;

namespace Example.MultiConnections;

public class TestPublishService : BackgroundService
{
    private readonly ILogger<TestPublishService> _logger;
    private readonly IRabbitHelper _rabbitHelper;

    public TestPublishService(ILogger<TestPublishService> logger, [FromKeyedServices("TestRabbitHelper")]IRabbitHelper rabbitHelper)
    {
        _logger = logger;
        _rabbitHelper = rabbitHelper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Testing TestPublishService");

        while (!stoppingToken.IsCancellationRequested)
        {
            _rabbitHelper.Publish("FooProducer", "Hello from test publish service.");
            await Task.Delay(1000, stoppingToken);
        }
    }
}