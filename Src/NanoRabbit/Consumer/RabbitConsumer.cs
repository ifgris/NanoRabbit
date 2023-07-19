using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.Consumer
{
    public abstract class RabbitConsumer<T> : IRabbitConsumer, IDisposable
    {
        private readonly IModel _channel;
        private readonly IRabbitPool _pool;
        private readonly ConsumerConfig _consumerConfig;

        public RabbitConsumer(string connectionName, string consumerName, IRabbitPool pool)
        {
            _pool = pool;
            var connection = _pool.GetConnection(connectionName);
            _channel = connection.CreateModel();
            _consumerConfig = _pool.GetConsumer(consumerName);
        }

        public void Receive()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<T>(body);
                MessageHandler(message);
            };
            _channel.BasicConsume(queue: _consumerConfig.QueueName, autoAck: true, consumer: consumer);
        }

        protected abstract void MessageHandler(T message);

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
