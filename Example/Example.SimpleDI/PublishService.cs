using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace Example.SimpleDI
{
    public class PublishService : BackgroundService
    {
        private readonly IRabbitPool _rabbitPool;
        private readonly ILogger<PublishService> _logger;
        private readonly IRabbitProducer _dataBasicQueueProducer;
        private readonly IRabbitProducer _hostBasicQueueProducer;

        public PublishService(IRabbitPool rabbitPool, ILogger<PublishService> logger, IRabbitProducer dataBasicQueueProducer, IRabbitProducer hostBasicQueueProducer)
        {
            _rabbitPool = rabbitPool;
            _logger = logger;
            //_producer = new RabbitProducer("Connection1", "DataBasicQueueProducer", _rabbitPool);
            _dataBasicQueueProducer = dataBasicQueueProducer;
            _hostBasicQueueProducer = hostBasicQueueProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _rabbitPool.SimplePublish("Connection1", "DataBasicQueueProducer", "Hello from conn1");
                //_dataBasicQueueProducer.Publish<string>("Hello from conn1");
                _logger.LogInformation("Conn 1");
                _rabbitPool.SimplePublish("Connection2", "HostBasicQueueProducer", "Hello from conn2");
                //_hostBasicQueueProducer.Publish<string>("Hello from conn2");
                _logger.LogInformation("Conn 2");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
