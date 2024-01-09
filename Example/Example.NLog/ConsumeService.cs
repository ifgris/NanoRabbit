using Microsoft.Extensions.Logging;
using NanoRabbit.Consumer;

namespace Example.NLog;

public class ConsumeService : RabbitSubscriberAsync
{
    private readonly ILogger<ConsumeService>? _logger;

    public ConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriberAsync>? logger, string consumerName, ILogger<ConsumeService>? logger2) : base(consumer, consumerName, logger)
    {
        _logger = logger2;
    }

    protected override Task HandleMessage(string message)
    {
        _logger?.LogInformation(message);
        return Task.CompletedTask;
    }
}