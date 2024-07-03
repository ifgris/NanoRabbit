﻿using NanoRabbit.Connection;
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
        private readonly Dictionary<string, AsyncEventingBasicConsumer> _asyncConsumers;
        private readonly RabbitConfiguration _rabbitConfig;

        public RabbitHelper(
            RabbitConfiguration rabbitConfig)
        {
            _rabbitConfig = rabbitConfig;

            var hostName = _rabbitConfig.HostName;
            var port = _rabbitConfig.Port;
            var virtualHost = _rabbitConfig.VirtualHost;
            var userName = _rabbitConfig.UserName;
            var password = _rabbitConfig.Password;
            var factory = new ConnectionFactory() { HostName = hostName, Port = port, VirtualHost = virtualHost, UserName = userName, Password = password };
            if (_rabbitConfig.UseAsyncConsumer) factory.DispatchConsumersAsync = true;
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumers = new Dictionary<string, EventingBasicConsumer>();
            _asyncConsumers = new Dictionary<string, AsyncEventingBasicConsumer>();
        }

        /// <summary>
        /// Get ProducerOptions.
        /// </summary>
        /// <param name="producerName"></param>
        /// <returns></returns>
        public ProducerOptions GetProducerOption(string producerName)
        {
            var connectionOption = _rabbitConfig.Producers.FirstOrDefault(o => o.ProducerName == producerName);

            if (connectionOption == null)
            {
                throw new Exception($"Producer: {producerName} not found!");
            }

            return connectionOption;
        }

        /// <summary>
        /// Get ConsumerOptions.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ConsumerOptions GetConsumerOption(string? consumerName)
        {
            var connectionOption = _rabbitConfig.Consumers.FirstOrDefault(x => x.ConsumerName == consumerName);

            if (connectionOption == null)
            {
                throw new Exception($"Consumer: {consumerName} not found!");
            }

            return connectionOption;
        }

        public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            _channel.QueueDeclare(queue: queueName, durable, exclusive, autoDelete, arguments);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        public void Publish<T>(string producerName, T message)
        {
            var option = GetProducerOption(producerName);

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

            _channel.BasicPublish(exchange: option.ExchangeName, routingKey: option.RoutingKey, basicProperties: properties, body: body);
        }

        /// <summary>
        /// Publish batch messages to queue(s)
        /// </summary>
        /// <param name="producerName"></param>
        /// <param name="messageList"></param>
        public void PublishBatch<T>(string producerName, IEnumerable<T?> messageList)
        {
            var option = GetProducerOption(producerName);

            var messageObjs = messageList.ToList();

            _channel.ExchangeDeclare(option.ExchangeName, option.Type,
                durable: option.Durable, autoDelete: option.AutoDelete,
                arguments: option.Arguments);
            var properties = _channel.CreateBasicProperties();

            foreach (var messageObj in messageObjs)
            {
                var messageStr = JsonConvert.SerializeObject(messageObj);
                var body = Encoding.UTF8.GetBytes(messageStr);

                _channel.BasicPublish(
                            exchange: option.ExchangeName,
                            routingKey: option.RoutingKey,
                            basicProperties: properties,
                            body: body);
            }
        }

        public void AddConsumer(string consumerName, Action<string> onMessageReceived, int consumers = 1)
        {
            var option = GetConsumerOption(consumerName);

            for (int i = 0; i < consumers; i++)
            {
                var consumerId = string.Concat(option.QueueName, "-", i + 1);
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

                    _channel.BasicConsume(queue: option.QueueName, autoAck: false, consumer: consumer);
                    _consumers[consumerId] = consumer;
                }
            }
        }

        public async Task AddAsyncConsumer(string consumerName, Func<string, Task> onMessageReceivedAsync, int consumers = 1)
        {
            var option = GetConsumerOption(consumerName);

            for (int i = 0; i < consumers; i++)
            {
                var consumerId = string.Concat(option.QueueName, "-", i + 1);
                if (!_asyncConsumers.ContainsKey(consumerId))
                {
                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        await onMessageReceivedAsync(message);

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        await Task.Yield();
                    };

                    _channel.BasicConsume(queue: option.QueueName, autoAck: false, consumer: consumer);
                    _asyncConsumers[consumerId] = consumer;
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