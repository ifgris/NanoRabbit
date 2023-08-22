using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace Example.ConsumerQuickStart;

public class QueueNameConsumer : RabbitConsumer<string>
{
    private readonly ILogger<RabbitConsumer<string>> _logger;

    public QueueNameConsumer(string queueName, IRabbitPool rabbitPool, ILogger<RabbitConsumer<string>>? logger) : base(
        queueName, rabbitPool, logger)
    {
        _logger = logger;
    }

    public override void MessageHandler(string message)
    {
        _logger.LogInformation($"ConsumerLogging: Receive: {message}");
    }
}