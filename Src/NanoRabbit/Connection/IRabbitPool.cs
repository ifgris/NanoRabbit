using RabbitMQ.Client;

namespace NanoRabbit.Connection
{
    public interface IRabbitPool
    {
        /// <summary>
        /// Get register connection by connectionName
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public IConnection GetConnection(string connectionName);
        /// <summary>
        /// Get registered producerConfig by producerName.
        /// </summary>
        /// <param name="producerName"></param>
        /// <returns></returns>
        public ProducerConfig GetProducer(string producerName);
        /// <summary>
        /// Get registered consumerConfig by consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        public ConsumerConfig GetConsumer(string consumerName);

        /// <summary>
        /// Register connection by connect options.
        /// </summary>
        /// <param name="options"></param>
        public void RegisterConnection(ConnectOptions options);

        /// <summary>
        /// Close All Connections.
        /// </summary>
        public void CloseAllConnections();

        /// <summary>
        /// Close Connection by connectionName.
        /// </summary>
        /// <param name="connectionName"></param>
        public void CloseConnection(string connectionName);

        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="producerName"></param>
        /// <param name="Message"></param>
        public void SimplePublish<T>(string connectionName, string producerName, T Message);

        /// <summary>
        /// Receive message from queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="consumerName"></param>
        /// <param name="messageHandler"></param>
        public void SimpleConsume<T>(string connectionName, string consumerName, Action<T> messageHandler);
    }
}
