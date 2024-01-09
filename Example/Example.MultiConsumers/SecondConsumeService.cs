using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.MultiConsumers;

public class SecondConsumeService : RabbitSubscriberAsync
{
    private readonly ILogger<SecondConsumeService> _logger;
    
    public SecondConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriberAsync>? logger, string consumerName, ILogger<SecondConsumeService> logger1) : base(consumer, consumerName, logger)
    {
        _logger = logger1;
    }

    protected override Task HandleMessage(string message)
    {
        _logger.LogInformation(message);
        return Task.CompletedTask;
    }
}