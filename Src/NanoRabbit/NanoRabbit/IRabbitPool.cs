using RabbitMQ.Client;

namespace NanoRabbit.NanoRabbit
{
    public interface IRabbitPool
    {
        public IConnection GetConnection(string connectionName);

        /// <summary>
        /// Register connection by connect options.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="options"></param>
        public void RegisterConnection(string connectionName, ConnectOptions options);

        /// <summary>
        /// Register connection by uri.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="connUri"></param>
        public void RegisterConnection(string connectionName, ConnectUri connUri);

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
        /// Original RabbitMQ BasicPublish.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="body"></param>
        public void Send(string connectionName, string exchangeName, string routingKey, byte[] body);

        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="Message"></param>
        public void Publish<T>(string connectionName, string exchangeName, string routingKey, T Message);

        /// <summary>
        /// Original RabbitMQ BasicConsume.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="queueName"></param>
        /// <param name="handler"></param>
        public void Receive(string connectionName, string queueName, Action<byte[]> handler);

    }
}
