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
        private readonly IFactoryManager _factoryManager;
        private readonly ConcurrentDictionary<string, Task<IConnection?>> _connections = new();
        private readonly ILogger<ConnectionManager> _logger;

        /// <summary>
        /// ConnectionManager Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="factoryManager"></param>
        public ConnectionManager(ILogger<ConnectionManager> logger, IFactoryManager factoryManager)
        {
            _logger = logger;
            _factoryManager = factoryManager;
        }

        /// <inheritdoc />
        public async Task<IConnection?> TryConnectAsync(string connectionName, ConnectionFactory factory,
            CancellationToken cancellationToken = default)
        {
            // Avoid creating connections repeatedly
            Task<IConnection?> connectionTask = _connections.GetOrAdd(connectionName, async (key) =>
            {
                IConnection? connection = null;
                try
                {
                    _logger.LogInformation(
                        "Creating new RabbitMQ connection for '{ConnectionName}' to {HostName}:{Port}...",
                        connectionName, factory.HostName, factory.Port);
                    connection = await factory.CreateConnectionAsync(cancellationToken);
                    connection.ConnectionShutdownAsync += (sender, args) =>
                    {
                        _logger.LogWarning("Connection '{ConnectionName}' shut down. Reason: {Reason}", connectionName,
                            args.ReplyText);
                        _connections.TryRemove(connectionName, out _);
                        return Task.CompletedTask;
                    };
                    _logger.LogInformation("RabbitMQ connection '{ConnectionName}' created successfully.",
                        connectionName);
                    return connection;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create RabbitMQ connection for '{ConnectionName}'.",
                        connectionName);
                    return connection;
                }
            });

            var connection = await connectionTask;

            // if (connection != null && !connection.IsOpen)
            // {
            //     _logger.LogWarning("Existing connection for '{ConnectionName}' is closed. Attempting to recreate...",
            //         connectionName);
            //     _connections.TryRemove(connectionName, out _);
            //     return await TryConnectAsync(connectionName, factory, cancellationToken);
            // }

            return connection;
        }

        /// <inheritdoc />
        public bool CheckConnection(IConnection? connection)
        {
            if (connection == null) return false;
            return connection.IsOpen;
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