using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.SimpleDI
{
    public class PublishService : BackgroundService
    {
        private readonly ILogger<PublishService> _logger;
        private readonly DataBasicQueueProducer _dataProducer;
        private readonly HostBasicQueueProducer _hostProducer;

        public PublishService(ILogger<PublishService> logger, DataBasicQueueProducer dataProducer, HostBasicQueueProducer hostProducer)
        {
            _logger = logger;
            _dataProducer = dataProducer;
            _hostProducer = hostProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _dataProducer.Enqueue("Hello from conn1");
                _logger.LogInformation("Conn 1");
                _hostProducer.Enqueue("Hello from conn2");
                _logger.LogInformation("Conn 2");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
