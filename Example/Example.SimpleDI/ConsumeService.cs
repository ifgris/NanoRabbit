using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;

namespace Example.SimpleDI
{
    public class ConsumeService : BackgroundService
    {
        private readonly FooFirstQueueConsumer _consumer;
        private readonly IRabbitPool _pool;

        public ConsumeService(IRabbitPool pool, FooFirstQueueConsumer consumer)
        {
            _pool = pool;
            // _consumer = new BasicConsumer("Connection1", "DataBasicQueueConsumer", _pool);
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // _consumer.StartSubscribing();
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
