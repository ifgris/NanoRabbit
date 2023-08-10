using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;

namespace Example.Autofac
{
    public class ConsumeService : BackgroundService
    {
        private readonly FooFirstQueueConsumer _consumer;
        private readonly IRabbitPool _pool;

        public ConsumeService(IRabbitPool pool, FooFirstQueueConsumer consumer)
        {
            _pool = pool;
            _consumer = consumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.StartConsuming();
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Dispose();
            return base.StartAsync(cancellationToken);
        }
    }
}
