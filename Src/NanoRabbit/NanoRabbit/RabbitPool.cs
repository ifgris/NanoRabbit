using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.NanoRabbit
{

    public class RabbitPool : IRabbitPool
    {
        private readonly IDictionary<string, IConnection> _connections = new Dictionary<string, IConnection>();

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
        /// Register connection by connect options.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="options"></param>
        public void RegisterConnection(string connectionName, ConnectOptions options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost
            };

            var connection = factory.CreateConnection();

            _connections.Add(connectionName, connection);
        }

        /// <summary>
        /// Register connection by uri.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="connUri"></param>
        public void RegisterConnection(string connectionName, ConnectUri connUri)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(connUri.ConnectionString)
            };

            var connection = factory.CreateConnection();

            _connections.Add(connectionName, connection);
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
        /// Original RabbitMQ BasicPublish.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="body"></param>
        public void Send(string connectionName, string exchangeName, string routingKey, byte[] body)
        {
            using (var channel = GetConnection(connectionName).CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchangeName, routingKey, properties, body);
            }
        }

        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="Message"></param>
        public void Publish<T>(string connectionName, string exchangeName, string routingKey, T Message)
        {
            using (var channel = GetConnection(connectionName).CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var messageString = JsonConvert.SerializeObject(Message);
                var messageBytes = Encoding.UTF8.GetBytes(messageString);

                channel.BasicPublish(exchangeName, routingKey, properties, messageBytes);
            }
        }

        /// <summary>
        /// Original RabbitMQ BasicConsume.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="queueName"></param>
        /// <param name="handler"></param>
        public void Receive(string connectionName, string queueName, Action<byte[]> handler)
        {
            using (var channel = GetConnection(connectionName).CreateModel())
            {
                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    //var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                    //var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    handler(ea.Body.ToArray());
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
            }
        }
    }
}
