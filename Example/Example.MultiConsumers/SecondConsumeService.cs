using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.MultiConsumers;

public class SecondConsumeService : RabbitAsyncSubscriber
{
    private readonly ILogger<SecondConsumeService> _logger;

    public SecondConsumeService(ILogger<SecondConsumeService> logger2, IRabbitConsumer consumer, string consumerName, ILogger<RabbitAsyncSubscriber>? logger, int consumerCount = 1) : base(consumer, consumerName, logger, consumerCount)
    {
        _logger = logger2;
    }

    protected override Task HandleMessageAsync(string message)
    {
        _logger.LogInformation(message);
        return Task.CompletedTask;
    }
}