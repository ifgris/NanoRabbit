using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;

namespace Example.SimpleDI
{
    public class ConsumeService : BackgroundService
    {
        private readonly BasicConsumer _consumer;
        private readonly IRabbitPool _pool;

        public ConsumeService(IRabbitPool pool)
        {
            _pool = pool;
            _consumer = new BasicConsumer("Connection1", "DataBasicQueueConsumer", _pool);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _consumer.Receive();
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
