using Microsoft.Extensions.Logging;
using NanoRabbit.Logging;
using NanoRabbit.Producer;
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

        private readonly ILogger<RabbitPool>? _logger;

        public static GlobalConfig? GlobalConfig { get; private set; }


        public RabbitPool(Action<GlobalConfig> config)
        {
            GlobalConfig = new GlobalConfig();
            config(GlobalConfig);
            if (GlobalConfig.EnableLogging == false)
            {
                _logger = null;
            }
            else
            {
                _logger = GlobalLogger.CreateLogger<RabbitPool>();
            }
        }
        public RabbitPool(ILogger<RabbitPool> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Register connection by connect options.
        /// </summary>
        /// <param name="options"></param>
        public void RegisterConnection(ConnectOptions options)
        {
            try
            {
                if (options.ConnectConfig != null)
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = options.ConnectConfig?.HostName,
                        Port = (options.ConnectConfig != null) ? options.ConnectConfig.Port : 5672,
                        UserName = options.ConnectConfig?.UserName,
                        Password = options.ConnectConfig?.Password,
                        VirtualHost = options.ConnectConfig?.VirtualHost
                    };

                    var connection = factory.CreateConnection();
                    _connections.Add(options.ConnectionName, connection);
                    _logger?.LogInformation($"Connection - {options.ConnectionName} Registered");
                }
                else if (options.ConnectUri != null)
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(options.ConnectUri.ConnectionString)
                    };

                    var connection = factory.CreateConnection();
                    _connections.Add(options.ConnectionName, connection);
                }

                if (options.ProducerConfigs != null)
                {
                    foreach (var producerConfig in options.ProducerConfigs)
                    {
                        _producerConfig.Add(producerConfig.ProducerName, producerConfig);
                    }
                }

                if (options.ConsumerConfigs != null)
                {
                    foreach (var consumerConfig in options.ConsumerConfigs)
                    {
                        _consumerConfig.Add(consumerConfig.ConsumerName, consumerConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                throw;
            }
        }

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
                channel.ExchangeDeclare(producerConfig.ExchangeName, producerConfig.Type, durable: producerConfig.Durable, autoDelete: producerConfig.AutoDelete, arguments: producerConfig.Arguments);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var messageString = JsonConvert.SerializeObject(message);
                var messageBytes = Encoding.UTF8.GetBytes(messageString);

                channel.BasicPublish(producerConfig.ExchangeName, producerConfig.RoutingKey, properties, messageBytes);
            }

            _logger?.LogInformation($"Message published by {producerName} to {connectionName}");
        }

        /// <summary>
        /// Receive message from queue.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="consumerName"></param>
        /// <param name="messageHandler"></param>
        public void SimpleReceive<T>(string connectionName, string consumerName, Action<T> messageHandler)
        {
            try
            {
                IModel channel = GetConnection(connectionName).CreateModel();
                ConsumerConfig consumerConfig = GetConsumer(consumerName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    _logger?.LogInformation($"Received messages from {connectionName} - {consumerConfig.QueueName}");
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonConvert.DeserializeObject<T>(body);
                    if (message != null)
                    {
                        messageHandler(message);
                    }
                };
                channel.BasicConsume(queue: consumerConfig.QueueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
            }
        }
    }
}
