using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.NanoRabbit;
using System.Text;

namespace Example.SimpleDI
{
    public class PublishService : BackgroundService
    {
        private readonly RabbitPool _rabbitPool;
        private readonly ILogger<PublishService> _logger;

        public PublishService(RabbitPool rabbitPool, ILogger<PublishService> logger)
        {
            _rabbitPool = rabbitPool;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _rabbitPool.Send("Connection1", "BASIC.TOPIC", "BASIC.KEY", Encoding.UTF8.GetBytes("Hello from conn1"));
                _logger.LogInformation("Conn 1");
                _rabbitPool.Send("Connection2", "BASIC.TOPIC", "BASIC.KEY", Encoding.UTF8.GetBytes("Hello from conn2"));
                _logger.LogInformation("Conn 2");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
