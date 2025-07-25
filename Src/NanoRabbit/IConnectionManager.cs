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
        /// Check the connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool CheckConnection(IConnection? connection);
    }
}