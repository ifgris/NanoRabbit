using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.ReadSettings;

public class ConsumeService : RabbitSubscriber
{
    private readonly ILogger<RabbitSubscriber>? _logger;
    
    public ConsumeService(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger = null) : base(consumer, consumerName, logger)
    {
        _logger = logger;
    }

    protected override bool HandleMessage(string message)
    {
        _logger?.LogInformation(message);
        return true;
    }
}