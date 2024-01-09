using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.MultiConsumers;

public class FirstConsumeService : RabbitSubscriber
{
    private readonly ILogger<RabbitSubscriber>? _logger;

    public FirstConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriber>? logger, string consumerName) : base(consumer, logger, consumerName)
    {
        _logger = logger;
    }

    protected override bool HandleMessage(string message)
    {
        _logger?.LogInformation(message);
        return true;
    }
}