namespace NanoRabbit.NanoRabbit
{
    public interface IRabbitProducer
    {
        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Message"></param>
        public void Publish<T>(T message);
    }
}
