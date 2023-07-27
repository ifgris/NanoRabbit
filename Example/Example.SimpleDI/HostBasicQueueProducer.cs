using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace Example.SimpleDI
{
    public class HostBasicQueueProducer : RabbitProducer
    {
        public HostBasicQueueProducer(string connectionName, string producerName, IRabbitPool pool) : base(connectionName, producerName, pool) { }
    }
}
