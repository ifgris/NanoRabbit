using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace NanoRabbit
{
    /// <summary>
    /// RabbitMQ Connection Manager
    /// </summary>
    public class ConnectionManager : IConnectionManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, Task<IConnection>> _connections = new();
        private readonly ILogger<ConnectionManager> _logger;

        /// <summary>
        /// ConnectionManager Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ConnectionManager(ILogger<ConnectionManager> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IConnection> GetOrCreateConnectionAsync(string connectionName, ConnectionFactory factory,
            CancellationToken cancellationToken = default)
        {
            // Avoid creating connections repeatedly
            Task<IConnection> connectionTask = _connections.GetOrAdd(connectionName, async (key) =>
            {
                try
                {
                    _logger.LogInformation(
                        "Creating new RabbitMQ connection for '{ConnectionName}' to {HostName}:{Port}...",
                        connectionName, factory.HostName, factory.Port);
                    var newConnection = await factory.CreateConnectionAsync(cancellationToken);
                    newConnection.ConnectionShutdownAsync += (sender, args) =>
                    {
                        _logger.LogWarning("Connection '{ConnectionName}' shut down. Reason: {Reason}", connectionName,
                            args.Cause);
                        _connections.TryRemove(connectionName, out _);
                        return Task.CompletedTask;
                    };
                    _logger.LogInformation("RabbitMQ connection '{ConnectionName}' created successfully.",
                        connectionName);
                    return newConnection;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create RabbitMQ connection for '{ConnectionName}'.",
                        connectionName);
                    _connections.TryRemove(connectionName, out _);
                    throw;
                }
            });

            var connection = await connectionTask;

            if (!connection.IsOpen)
            {
                _logger.LogWarning("Existing connection for '{ConnectionName}' is closed. Attempting to recreate...",
                    connectionName);
                _connections.TryRemove(connectionName, out _);
                return await GetOrCreateConnectionAsync(connectionName, factory, cancellationToken);
            }

            return connection;
        }

        public void Dispose()
        {
            foreach (var connectionEntry in _connections)
            {
                var connection = connectionEntry.Value.ConfigureAwait(false).GetAwaiter().GetResult();
                if (connection.IsOpen)
                {
                    try
                    {
                        connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        _logger.LogInformation("Closed RabbitMQ connection '{ConnectionName}'.", connectionEntry.Key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error closing RabbitMQ connection '{ConnectionName}'.",
                            connectionEntry.Key);
                    }
                }

                connection.Dispose();
            }

            _connections.Clear();
        }
    }
}