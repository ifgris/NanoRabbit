using System.Threading.Channels;

namespace NanoRabbit.Producer
{
    public interface IRabbitProducer
    {
        /// <summary>
        /// Add message to the end of concurrent cache queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Enqueue<T>(T message);
        
        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Publish<T>(T message);
        
        /// <summary>
        /// Dispose connection.
        /// </summary>
        public void Dispose();
    }
}
