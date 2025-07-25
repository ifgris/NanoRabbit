using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace NanoRabbit
{
    public class FactoryManager : IFactoryManager
    {
        private readonly ConcurrentDictionary<string, ConnectionFactory> _factories = new();
        
        /// <inheritdoc />
        public bool TryAddFactory(string connectionName, ConnectionFactory factory,
            CancellationToken cancellationToken = default)
        {
            return _factories.TryAdd(connectionName, factory);
        }
    }
}