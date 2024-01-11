using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.MultiConsumers;

public class SecondConsumeService : RabbitAsyncSubscriber
{
    private readonly ILogger<SecondConsumeService> _logger;
    
    public SecondConsumeService(IRabbitConsumer consumer, string consumerName, ILogger<RabbitAsyncSubscriber>? logger, ILogger<SecondConsumeService> logger2) : base(consumer, consumerName, logger)
    {
        _logger = logger2;
    }

    protected override Task HandleMessage(string message)
    {
        _logger.LogInformation(message);
        return Task.CompletedTask;
    }
}