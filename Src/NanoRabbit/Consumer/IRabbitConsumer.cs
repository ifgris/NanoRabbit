namespace NanoRabbit.Consumer
{
    /// <summary>
    /// IRabbitConsumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRabbitConsumer<T> : IDisposable
    {
        /// <summary>
        /// Receive from Queue.
        /// </summary>
        void Receive();

        /// <summary>
        /// Handle the received message.
        /// </summary>
        /// <param name="message"></param>
        void MessageHandler(T message);
    }
}
