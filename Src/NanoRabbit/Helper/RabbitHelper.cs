using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.Helper
{
    public class RabbitHelper : IRabbitHelper, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Dictionary<string, EventingBasicConsumer> _consumers;

        public RabbitHelper(
            string hostname,
            int port = 5672,
            string virtualHost = "/",
            string userName = "guest",
            string password = "guest")
        {
            var factory = new ConnectionFactory() { HostName = hostname, Port = port, VirtualHost = virtualHost, UserName = userName, Password = password };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumers = new Dictionary<string, EventingBasicConsumer>();
        }

        public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            _channel.QueueDeclare(queue: queueName, durable, exclusive, autoDelete, arguments);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        public void Publish<T>(string exchangeName, string routingKey, T message)
        {
            string messageStr;

            if (typeof(T) == typeof(string))
            {
                messageStr = message.ToString();
            }
            else
            {
                messageStr = JsonConvert.SerializeObject(message);
            }
            var body = Encoding.UTF8.GetBytes(messageStr);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
        }

        public void AddConsumer(string queueName, Action<string> onMessageReceived, int consumers = 1)
        {
            for (int i = 0; i < consumers; i++)
            {
                var consumerName = string.Concat(queueName, "-", i + 1);
                if (!_consumers.ContainsKey(consumerName))
                {
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        onMessageReceived(message);

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };

                    _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                    _consumers[queueName] = consumer;
                }
            }
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers.Values)
            {
                foreach (var consumerTag in consumer.ConsumerTags)
                {
                    _channel.BasicCancel(consumerTag);
                }
            }
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

}
