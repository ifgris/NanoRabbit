using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.NLog
{
    public class PublishService : BackgroundService
    {
        private readonly ILogger<PublishService> _logger;
        private readonly FooFirstQueueProducer _dataProducer;
        private readonly BarFirstQueueProducer _hostProducer;

        public PublishService(ILogger<PublishService> logger, FooFirstQueueProducer dataProducer, BarFirstQueueProducer hostProducer)
        {
            _logger = logger;
            _dataProducer = dataProducer;
            _hostProducer = hostProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Testing PublishService");

            while (!stoppingToken.IsCancellationRequested)
            {
                _dataProducer.Publish("Hello from conn1");
                _hostProducer.Enqueue("Hello from conn2");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
