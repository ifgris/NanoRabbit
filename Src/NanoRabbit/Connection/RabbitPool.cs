using Microsoft.Extensions.Logging;
using NanoRabbit.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.Connection
{
    /// <summary>
    /// RabbitPool, configured with the settings of RabbitMQ. All Configures in one.
    /// </summary>
    public class RabbitPool : IRabbitPool
    {
        private readonly IDictionary<string, IConnection> _connections = new Dictionary<string, IConnection>();
        private readonly IList<ConnectOptions> _options = new List<ConnectOptions>();
        private readonly IDictionary<string, ProducerConfig> _producerConfig = new Dictionary<string, ProducerConfig>();
        private readonly IDictionary<string, ConsumerConfig> _consumerConfig = new Dictionary<string, ConsumerConfig>();
        private readonly ILogger<RabbitPool>? _logger;
        
        private static GlobalConfig? GlobalConfig { get; set; }
        
        /// <summary>
        /// RabbitPool constructor with GlobalConfig Action
        /// </summary>
        /// <param name="config"></param>
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
        
        /// <summary>
        /// RabbitPool Constructor with Logger
        /// </summary>
        /// <param name="logger"></param>
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
                        Port = options.ConnectConfig?.Port ?? 5672,
                        UserName = options.ConnectConfig?.UserName,
                        Password = options.ConnectConfig?.Password,
                        VirtualHost = options.ConnectConfig?.VirtualHost
                    };

                    var connection = factory.CreateConnection();
                    
                    // register connection closed event
                    connection.ConnectionShutdown += (sender, args) =>
                    {
                        // try to reconnect rabbitmq
                        while (!connection.IsOpen)
                        {
                            try
                            {
                                connection = factory.CreateConnection();
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, ex.Message);
                                Thread.Sleep(1000 * 10);
                            }
                        }
                    };
                    
                    _connections.Add(options.ConnectionName, connection);
                    _logger?.LogInformation("Connection - {OptionsConnectionName} Registered", options.ConnectionName);
                }
                else if (options.ConnectUri != null)
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(options.ConnectUri.ConnectionString)
                    };

                    var connection = factory.CreateConnection();
                    
                    // register connection closed event
                    connection.ConnectionShutdown += (sender, args) =>
                    {
                        // try to reconnect rabbitmq
                        while (!connection.IsOpen)
                        {
                            try
                            {
                                connection = factory.CreateConnection();
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, ex.Message);
                                Thread.Sleep(1000 * 10);
                            }
                        }
                    };
                    
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
                
                _options.Add(options);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Get connection name and consumer config name by queue name.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public (string, string) GetConfigsByQueueName(string queueName)
        {
            var connectionName = _options.First(x =>
                    x.ConsumerConfigs != null && x.ConsumerConfigs.First(y => y.QueueName == queueName).QueueName ==
                    queueName).ConnectionName;
            var consumerConfigs = _options.First(x => x.ConnectionName == connectionName).ConsumerConfigs;
            if (consumerConfigs != null)
            {
                var consumerConfigName = consumerConfigs
                    .First(y => y.QueueName == queueName).ConsumerName;

                return (connectionName, consumerConfigName);
            }
            throw new ArgumentException($"Configs for {queueName} not found.");
        }

        /// <summary>
        /// Get registered connection by connectionName.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
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
        public ProducerConfig GetProducerConfig(string producerName)
        {
            if (!_producerConfig.ContainsKey(producerName))
            {
                throw new ArgumentException($"ProducerConfig {producerName} not found.");
            }

            return _producerConfig[producerName];
        }

        /// <summary>
        /// Get registered consumerConfig by consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        public ConsumerConfig GetConsumerConfig(string consumerName)
        {
            if (!_consumerConfig.ContainsKey(consumerName))
            {
                throw new ArgumentException($"ConsumerConfig {consumerName} not found.");
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
        public void NanoPublish<T>(string connectionName, string producerName, T message)
        {
            try
            {
                var producerConfig = GetProducerConfig(producerName);
    
                using (var channel = GetConnection(connectionName).CreateModel())
                {
                    channel.ExchangeDeclare(producerConfig.ExchangeName, producerConfig.Type, durable: producerConfig.Durable, autoDelete: producerConfig.AutoDelete, arguments: producerConfig.Arguments);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
    
                    var messageString = JsonConvert.SerializeObject(message);
                    var messageBytes = Encoding.UTF8.GetBytes(messageString);
    
                    channel.BasicPublish(producerConfig.ExchangeName, producerConfig.RoutingKey, properties, messageBytes);
                }
    
                _logger?.LogInformation("Message published by {ProducerName} to {ConnectionName}", producerName, connectionName);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
            }
        }

        /// <summary>
        /// Consume message from queue.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="consumerName"></param>
        /// <param name="messageHandler"></param>
        public void NanoConsume<T>(string connectionName, string consumerName, Action<T> messageHandler)
        {
            try
            {
                IModel channel = GetConnection(connectionName).CreateModel();
                ConsumerConfig consumerConfig = GetConsumerConfig(consumerName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    _logger?.LogInformation("Received messages from {ConnectionName} - {ConsumerConfigQueueName}", connectionName, consumerConfig.QueueName);
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
        
        /// <summary>
        /// Asynchronous Consume message from queue.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="consumerName"></param>
        /// <param name="messageHandler"></param>
        public async Task NanoConsumeAsync<T>(string connectionName, string consumerName, Action<T> messageHandler)
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory();
                // ...
                // use async-oriented consumer dispatcher. Only compatible with IAsyncBasicConsumer implementations
                factory.DispatchConsumersAsync = true;
                
                IModel channel = GetConnection(connectionName).CreateModel();
                ConsumerConfig consumerConfig = GetConsumerConfig(consumerName);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (ch, ea) =>
                {
                    _logger?.LogInformation("Received messages from {ConnectionName} - {ConsumerConfigQueueName}", connectionName, consumerConfig.QueueName);
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonConvert.DeserializeObject<T>(body);
                    if (message != null)
                    {
                        messageHandler(message);
                    }

                    await Task.Yield();
                };
                channel.BasicConsume(queue: consumerConfig.QueueName, autoAck: true, consumer: consumer);
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Forward message method.
        /// </summary>
        /// <param name="fromConnectionName"></param>
        /// <param name="fromComsumerName"></param>
        /// <param name="toConnectionName"></param>
        /// <param name="toProducerName"></param>
        /// <typeparam name="T"></typeparam>
        public void NanoForward<T>(string fromConnectionName, string fromComsumerName, string toConnectionName,
            string toProducerName)
        {
            try
            {
                IModel channel = GetConnection(fromConnectionName).CreateModel();
                ConsumerConfig consumerConfig = GetConsumerConfig(fromComsumerName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    _logger?.LogInformation("Received messages from {ConnectionName} - {ConsumerConfigQueueName}", fromConnectionName, consumerConfig.QueueName);
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonConvert.DeserializeObject<T>(body);
                    if (message != null)
                    {
                        NanoPublish<T>(toConnectionName, toProducerName, message); // publish message
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
