using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace Example.ProducerInConsumer
{
    public class FooFirstQueueConsumer : RabbitConsumer<string>
    {
        private readonly ILogger _logger;
        private readonly BarFirstQueueProducer _barFirstQueueProducer;

        public FooFirstQueueConsumer(string connectionName, string consumerName, IRabbitPool pool, ILogger<RabbitConsumer<string>> logger, BarFirstQueueProducer barFirstQueueProducer) : base(connectionName, consumerName, pool, logger)
        {
            _logger = logger;
            _barFirstQueueProducer = barFirstQueueProducer;
        }

        public override void MessageHandler(string message)
        {
            _logger.LogInformation($"ConsumerLogging: Receive: {message}");

            var newMessage = "new" + message;
            _barFirstQueueProducer.Enqueue(newMessage);
        }
    }
}