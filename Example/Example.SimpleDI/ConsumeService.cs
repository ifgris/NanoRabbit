using Microsoft.Extensions.Hosting;
using NanoRabbit.NanoRabbit;

namespace Example.SimpleDI
{
    public class ConsumeService : BackgroundService
    {
        private readonly BasicConsumer _consumer;
        private readonly RabbitPool _pool;

        public ConsumeService(RabbitPool pool)
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
