using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.ReadSettings;

public class ConsumeService : RabbitSubscriber
{
    private readonly ILogger<RabbitSubscriber>? _logger;

    public ConsumeService(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger = null, int consumerCount = 1) : base(consumer, consumerName, logger, consumerCount)
    {
        _logger = logger;
    }

    protected override bool HandleMessage(string message)
    {
        _logger?.LogInformation(message);
        return true;
    }
}