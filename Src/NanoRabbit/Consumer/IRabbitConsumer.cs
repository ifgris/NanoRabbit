namespace NanoRabbit.Consumer
{
    /// <summary>
    /// IRabbitConsumer
    /// </summary>
    public interface IRabbitConsumer : IDisposable
    {
        /// <summary>
        /// Receive from Queue.
        /// </summary>
        void Receive();

        /// <summary>
        /// Handle the received message.
        /// </summary>
        /// <param name="message"></param>
        void MessageHandler(object message);

        /// <summary>
        /// Start consumer thread
        /// </summary>
        void StartSubscribing();
    }
}
