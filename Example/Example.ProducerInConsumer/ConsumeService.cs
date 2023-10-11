using Microsoft.Extensions.Hosting;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;
using NanoRabbit.Producer;

namespace Example.ProducerInConsumer;

public class ConsumeService : BackgroundService
{
    private readonly RabbitConsumer _consumer;
    private readonly RabbitProducer _producer;

    public ConsumeService(RabbitConsumer consumer, RabbitProducer producer)
    {
        _consumer = consumer;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _consumer.Receive("BarFirstQueueConsumer", message => { _producer.Publish("FooSecondQueueProducer", message); });
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
}