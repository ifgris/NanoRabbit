using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace Example.SimpleDI
{
    public class DataBasicQueueProducer : RabbitProducer
    {
        public DataBasicQueueProducer(string connectionName, string producerName, IRabbitPool pool) : base(connectionName, producerName, pool) { }
    }
}
