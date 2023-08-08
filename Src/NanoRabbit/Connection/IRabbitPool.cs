using RabbitMQ.Client;

namespace NanoRabbit.Connection
{
    /// <summary>
    /// RabbitPool Interface
    /// </summary>
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
        /// <param name="message"></param>
        public void NanoPublish<T>(string connectionName, string producerName, T message);

        /// <summary>
        /// Consume message from queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="consumerName"></param>
        /// <param name="messageHandler"></param>
        public void NanoConsume<T>(string connectionName, string consumerName, Action<T> messageHandler);
        
        /// <summary>
        /// Asynchronous Consume message from queue.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="consumerName"></param>
        /// <param name="messageHandler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task NanoConsumeAsync<T>(string connectionName, string consumerName, Action<T> messageHandler);

        /// <summary>
        /// Forward message method.
        /// </summary>
        /// <param name="fromConnectionName"></param>
        /// <param name="fromComsumerName"></param>
        /// <param name="toConnectionName"></param>
        /// <param name="toProducerName"></param>
        /// <typeparam name="T"></typeparam>
        public void NanoForward<T>(string fromConnectionName, string fromComsumerName, string toConnectionName,
            string toProducerName);
    }
}
