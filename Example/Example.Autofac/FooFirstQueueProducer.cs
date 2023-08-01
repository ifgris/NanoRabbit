using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace Example.Autofac
{
    public class FooFirstQueueProducer : RabbitProducer
    {
        public FooFirstQueueProducer(string connectionName, string producerName, IRabbitPool pool, ILogger<RabbitProducer> logger) : base(connectionName, producerName, pool, logger)
        {
        }
    }
}
