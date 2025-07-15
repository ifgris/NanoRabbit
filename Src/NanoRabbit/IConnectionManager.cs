using RabbitMQ.Client;

namespace NanoRabbit
{
    public interface IConnectionManager
    {
        Task<IConnection> GetOrCreateConnectionAsync(string connectionName, ConnectionFactory factory, CancellationToken cancellationToken = default);
    }
}