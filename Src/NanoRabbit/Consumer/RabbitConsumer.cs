using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.Consumer
{
    /// <summary>
    /// RabbitConsumer, can be inherited by custom Consumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RabbitConsumer<T> : IRabbitConsumer<T>
    {
        private readonly IModel _channel;
        private readonly IRabbitPool _pool;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ILogger<RabbitConsumer<T>> _logger;

        private readonly Thread _consumeThread;

        public RabbitConsumer(string connectionName, string consumerName, IRabbitPool pool, ILogger<RabbitConsumer<T>> logger)
        {
            _pool = pool;
            _channel = _pool.GetConnection(connectionName).CreateModel();
            _consumerConfig = _pool.GetConsumer(consumerName);
            _consumeThread = new Thread(ReceiveTask);
            _consumeThread.Start();
            _logger = logger;
        }

        /// <summary>
        /// ReceiveTask runs in ConsumeThread.
        /// </summary>
        public void ReceiveTask()
        {
            var consumer = new EventingBasicConsumer(_channel);
            while (true)
            {
                try
                {
                    consumer.Received += (model, ea) =>
                    {
                        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonConvert.DeserializeObject<T>(body);
                        if (message != null)
                        {
                            MessageHandler(message);
                        }
                    };
                    _channel.BasicConsume(queue: _consumerConfig.QueueName, autoAck: true, consumer: consumer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Receive message from queue.
        /// </summary>
        public void Receive()
        {
            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonConvert.DeserializeObject<T>(body);
                    if (message != null)
                    {
                        MessageHandler(message);
                    }
                };
                _channel.BasicConsume(queue: _consumerConfig.QueueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Handle with the received message.
        /// </summary>
        /// <param name="message"></param>
        public abstract void MessageHandler(T message);

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
