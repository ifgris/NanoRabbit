using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NanoRabbit.NanoRabbit
{

    public class RabbitPool
    {
        private readonly IDictionary<string, IConnection> _connections = new Dictionary<string, IConnection>();

        public IConnection GetConnection(string connectionName)
        {
            if (!_connections.ContainsKey(connectionName))
            {
                throw new ArgumentException($"Connection {connectionName} not found.");
            }

            return _connections[connectionName];
        }

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

        public void CloseAllConnections()
        {
            foreach (var connection in _connections.Values)
            {
                connection.Close();
            }

            _connections.Clear();
        }

        public void CloseConnection(string connectionName)
        {
            _connections[connectionName].Close();

            _connections.Remove(connectionName);
        }

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
