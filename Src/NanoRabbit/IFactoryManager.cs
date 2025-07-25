using RabbitMQ.Client;

namespace NanoRabbit
{
    public interface IFactoryManager
    {
        /// <summary>
        /// Try to add RabbitMQ factory
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="factory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>ConnectionFactory</returns>
        public bool TryAddFactory(string connectionName, ConnectionFactory factory, CancellationToken cancellationToken = default);
    }
}