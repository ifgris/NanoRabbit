using NanoRabbit.Producer;

namespace Example.SimpleDI
{
    public class DataBasicQueueProducer : IRabbitProducer
    {
        private readonly IRabbitProducer _producer;

        public DataBasicQueueProducer(IRabbitProducer producer)
        {
            _producer = producer;
        }

        void IRabbitProducer.Publish<T>(T message)
        {
            _producer.Publish<T>(message);
        }
    }
}
