using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;

namespace Example.ProducerInConsumer
{
    public class ConsumeService : BackgroundService
    {
        private readonly FooFirstQueueConsumer _consumer;

        public ConsumeService(FooFirstQueueConsumer consumer)
        {
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
