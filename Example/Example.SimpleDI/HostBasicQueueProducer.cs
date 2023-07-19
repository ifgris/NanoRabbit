using NanoRabbit.Producer;

namespace Example.SimpleDI
{
    public class HostBasicQueueProducer : IRabbitProducer
    {
        private readonly IRabbitProducer _producer;

        public HostBasicQueueProducer(IRabbitProducer producer)
        {
            _producer = producer;
        }

        void IRabbitProducer.Publish<T>(T message)
        {
            _producer.Publish<T>(message);
        }
    }
}
