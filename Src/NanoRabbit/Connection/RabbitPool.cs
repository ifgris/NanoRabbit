using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.Connection
{

    public class RabbitPool : IRabbitPool
    {
        private readonly IDictionary<string, IConnection> _connections = new Dictionary<string, IConnection>();
        private readonly IDictionary<string, ProducerConfig> _producerConfig = new Dictionary<string, ProducerConfig>();
        private readonly IDictionary<string, ConsumerConfig> _consumerConfig = new Dictionary<string, ConsumerConfig>();

        /// <summary>
        /// Get registered connection by connectionName.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IConnection GetConnection(string connectionName)
        {
            if (!_connections.ContainsKey(connectionName))
            {
                throw new ArgumentException($"Connection {connectionName} not found.");
            }

            return _connections[connectionName];
        }

        /// <summary>
        /// Get registered producerConfig by producerName.
        /// </summary>
        /// <param name="producerName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ProducerConfig GetProducer(string producerName)
        {
            if (!_producerConfig.ContainsKey(producerName))
            {
                throw new ArgumentException($"Connection {producerName} not found.");
            }

            return _producerConfig[producerName];
        }

        /// <summary>
        /// Get registered consumerConfig by consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ConsumerConfig GetConsumer(string consumerName)
        {
            if (!_consumerConfig.ContainsKey(consumerName))
            {
                throw new ArgumentException($"Connection {consumerName} not found.");
            }

            return _consumerConfig[consumerName];
        }

        /// <summary>
        /// Register connection by connect options.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="options"></param>
        public void RegisterConnection(string connectionName, ConnectOptions options)
        {
            if (options.ConnectConfig != null)
            {
                var factory = new ConnectionFactory
                {
                    HostName = options.ConnectConfig?.HostName,
                    Port = options.ConnectConfig.Port,
                    UserName = options.ConnectConfig?.UserName,
                    Password = options.ConnectConfig?.Password,
                    VirtualHost = options.ConnectConfig?.VirtualHost
                };

                var connection = factory.CreateConnection();
                _connections.Add(connectionName, connection);
            }
            else if (options.ConnectUri != null)
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(options.ConnectUri.ConnectionString)
                };

                var connection = factory.CreateConnection();
                _connections.Add(connectionName, connection);
            }

            if (options.ProducerConfigs != null)
            {
                foreach (var key in options.ProducerConfigs.Keys)
                {
                    _producerConfig.Add(key, options.ProducerConfigs[key]);
                }
            }

            if (options.ConsumerConfigs != null)
            {
                foreach (var key in options.ConsumerConfigs.Keys)
                {
                    _consumerConfig.Add(key, options.ConsumerConfigs[key]);
                }
            }
        }

        /// <summary>
        /// Close All Connections.
        /// </summary>
        public void CloseAllConnections()
        {
            foreach (var connection in _connections.Values)
            {
                connection.Close();
            }

            _connections.Clear();
        }

        /// <summary>
        /// Close Connection by connectionName.
        /// </summary>
        /// <param name="connectionName"></param>
        public void CloseConnection(string connectionName)
        {
            _connections[connectionName].Close();

            _connections.Remove(connectionName);
        }

        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="producerName"></param>
        /// <param name="message"></param>
        public void SimplePublish<T>(string connectionName, string producerName, T message)
        {
            var producerConfig = GetProducer(producerName);

            using (var channel = GetConnection(connectionName).CreateModel())
            {
                channel.ExchangeDeclare(producerConfig.ExchangeName, ExchangeType.Topic, durable: true);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var messageString = JsonConvert.SerializeObject(message);
                var messageBytes = Encoding.UTF8.GetBytes(messageString);

                channel.BasicPublish(producerConfig.ExchangeName, producerConfig.RoutingKey, properties, messageBytes);
            }
        }
    }
}
