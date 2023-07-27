using NanoRabbit.Connection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;

namespace NanoRabbit.Producer
{
    public class RabbitProducer : IRabbitProducer
    {
        private readonly IRabbitPool _pool;
        private readonly ProducerConfig _producerConfig;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly ConcurrentQueue<object> _cacheQueue = new();
        private readonly Thread _publishThread;

        public RabbitProducer(string connectionName, string producerName, IRabbitPool pool)
        {
            _pool = pool;
            _producerConfig = _pool.GetProducer(producerName);
            _connection = _pool.GetConnection(connectionName);
            _channel = _connection.CreateModel();
            _publishThread = new Thread(SendData);
            _publishThread.Start();
        }

        /// <summary>
        /// SendData runs in PublishThread.
        /// </summary>
        public void SendData()
        {
            while(true)
            {
                // Send message when _cacheQueue is not empty
                if (_cacheQueue.TryDequeue(out object? message))
                {
                    // publish string data directly
                    if (message is string str)
                    {
                        var body = Encoding.UTF8.GetBytes(str);
                        _channel.BasicPublish(exchange: _producerConfig.ExchangeName, routingKey: _producerConfig.RoutingKey, basicProperties: null, body: body);
                    }
                    // serialize other type of data and publish
                    else
                    {
                        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                        _channel.BasicPublish(exchange: _producerConfig.ExchangeName, routingKey: _producerConfig.RoutingKey, basicProperties: null, body: body);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Add message to the end of concurrent cache queue.
        /// </summary>
        /// <param name="message"></param>
        public void Enqueue<T>(T message)
        {
            _cacheQueue.Enqueue(message!);
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
                channel.ExchangeDeclare(_producerConfig.ExchangeName, _producerConfig.Type, durable: _producerConfig.Durable);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var messageString = JsonConvert.SerializeObject(message);
                var messageBytes = Encoding.UTF8.GetBytes(messageString);

                channel.BasicPublish(_producerConfig.ExchangeName, _producerConfig.RoutingKey, properties, messageBytes);
            }
        }

        /// <summary>
        /// Dispose connection.
        /// </summary>
        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
