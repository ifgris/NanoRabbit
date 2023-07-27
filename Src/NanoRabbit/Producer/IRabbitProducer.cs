using System.Threading.Channels;

namespace NanoRabbit.Producer
{
    public interface IRabbitProducer
    {
        /// <summary>
        /// SendData runs in PublishThread.
        /// </summary>
        public void SendData();
        /// <summary>
        /// Add message to the end of concurrent cache queue.
        /// </summary>
        /// <param name="T"></param>
        public void Enqueue<T>(T message);
        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Message"></param>
        public void Publish<T>(T message);
        /// <summary>
        /// Dispose connection.
        /// </summary>
        public void Dispose();
    }
}
