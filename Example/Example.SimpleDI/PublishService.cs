using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;

namespace Example.SimpleDI
{
    public class PublishService : BackgroundService
    {
        private readonly IRabbitPool _rabbitPool;
        private readonly ILogger<PublishService> _logger;

        public PublishService(IRabbitPool rabbitPool, ILogger<PublishService> logger)
        {
            _rabbitPool = rabbitPool;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _rabbitPool.SimplePublish("Connection1", "DataBasicQueueProducer", "Hello from conn1");
                _logger.LogInformation("Conn 1");
                _rabbitPool.SimplePublish("Connection2", "HostBasicQueueProducer", "Hello from conn2");
                _logger.LogInformation("Conn 2");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
