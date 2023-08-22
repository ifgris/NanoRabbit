namespace NanoRabbit.Consumer
{
    /// <summary>
    /// IRabbitConsumer
    /// </summary>
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

        /// <summary>
        /// Start consumer thread
        /// </summary>
        void StartConsuming();
    }
}
