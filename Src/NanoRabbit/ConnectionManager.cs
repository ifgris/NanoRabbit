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

        public ConnectionManager(ILogger<ConnectionManager> logger)
        {
            _logger = logger;
        }

        public async Task<IConnection> GetOrCreateConnectionAsync(string connectionName, ConnectionFactory factory,
            CancellationToken cancellationToken = default)
        {
            // 使用 GetOrAdd 保证原子性，避免重复创建连接
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
                        // 当连接关闭时，从字典中移除，以便下次重新创建
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
                    _connections.TryRemove(connectionName, out _); // 失败时移除，确保下次重试
                    throw;
                }
            });

            var connection = await connectionTask;

            // 检查连接是否仍然开放，如果连接已经关闭（例如由于网络问题），则需要重新创建
            if (!connection.IsOpen)
            {
                _logger.LogWarning("Existing connection for '{ConnectionName}' is closed. Attempting to recreate...",
                    connectionName);
                // 尝试移除旧的，并重新调用 GetOrAdd 来创建新的
                _connections.TryRemove(connectionName, out _);
                return await GetOrCreateConnectionAsync(connectionName, factory, cancellationToken); // 递归调用以重新创建
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