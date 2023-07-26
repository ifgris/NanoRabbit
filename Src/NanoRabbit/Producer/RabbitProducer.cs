using NanoRabbit.Connection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace NanoRabbit.Producer
{
    public class RabbitProducer : IRabbitProducer
    {
        private readonly IRabbitPool _pool;
        private readonly ProducerConfig _producerConfig;
        private readonly IConnection _connection;

        public RabbitProducer(string connectionName, string producerName, IRabbitPool pool)
        {
            _pool = pool;
            _producerConfig = _pool.GetProducer(producerName);
            _connection = _pool.GetConnection(connectionName);
        }


        /// <summary>
        /// Publish Any Types of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Publish<T>(T message)
        {
            using (var channel = _connection.CreateModel())
            {
                channel.ExchangeDeclare(_producerConfig.ExchangeName, _producerConfig.Type, durable: true);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var messageString = JsonConvert.SerializeObject(message);
                var messageBytes = Encoding.UTF8.GetBytes(messageString);

                channel.BasicPublish(_producerConfig.ExchangeName, _producerConfig.RoutingKey, properties, messageBytes);
            }
        }
    }
}
