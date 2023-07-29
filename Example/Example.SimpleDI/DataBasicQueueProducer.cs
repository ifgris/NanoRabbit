using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace Example.SimpleDI
{
    public class DataBasicQueueProducer : RabbitProducer
    {
        public DataBasicQueueProducer(string connectionName, string producerName, IRabbitPool pool, ILogger<RabbitProducer> logger) : base(connectionName, producerName, pool, logger)
        {
        }
    }
}
