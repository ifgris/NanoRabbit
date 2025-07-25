using RabbitMQ.Client;

namespace NanoRabbit
{
    /// <summary>
    /// Connection manager interface
    /// </summary>
    public interface IConnectionManager

    {
        /// <summary>
        /// Get or create Connection
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="factory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IConnection?> TryConnectAsync(string connectionName, ConnectionFactory factory,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Reconnect Connection
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IConnection?> TryReconnectAsync(string connectionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check the connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool CheckConnection(IConnection? connection);
        
        /// <summary>
        /// Try to add RabbitMQ factory
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="factory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>ConnectionFactory</returns>
        public bool TryAddFactory(string connectionName, ConnectionFactory factory, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Try to get RabbitMQ ConnectionFactory
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ConnectionFactory? TryGetFactory(string connectionName, CancellationToken cancellationToken = default);
    }
}