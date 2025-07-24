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
        Task<IConnection> GetOrCreateConnectionAsync(string connectionName, ConnectionFactory factory, CancellationToken cancellationToken = default);
    }
}