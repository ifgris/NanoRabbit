using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit
{
    /// <summary>
    /// RabbitHelper
    /// </summary>
    public class RabbitHelper : IRabbitHelper, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Dictionary<string, EventingBasicConsumer> _consumers;
        private readonly Dictionary<string, AsyncEventingBasicConsumer> _asyncConsumers;
        private readonly RabbitConfiguration _rabbitConfig;
        private readonly ILogger _logger;
        private readonly ResiliencePipeline _pipeline;

        /// <summary>
        /// RabbitHelper constructor.
        /// </summary>
        /// <param name="rabbitConfig"></param>
        /// <param name="logger"></param>
        public RabbitHelper(RabbitConfiguration rabbitConfig, ILogger logger)
        {
            _rabbitConfig = rabbitConfig;
            ConnectionFactory factory = new();

            if (!string.IsNullOrEmpty(_rabbitConfig.Uri))
            {
                factory.Uri = new Uri(_rabbitConfig.Uri);
            }
            else
            {
                factory = new ConnectionFactory
                {
                    HostName = _rabbitConfig.HostName,
                    Port = _rabbitConfig.Port,
                    VirtualHost = _rabbitConfig.VirtualHost,
                    UserName = _rabbitConfig.UserName,
                    Password = _rabbitConfig.Password
                };
            }

            if (_rabbitConfig.UseAsyncConsumer) factory.DispatchConsumersAsync = true;

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _consumers = new Dictionary<string, EventingBasicConsumer>();
            _asyncConsumers = new Dictionary<string, AsyncEventingBasicConsumer>();
            _logger = logger;

            _pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 }) // Add retry using the default options
                .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 seconds timeout
                .Build(); // Builds the resilience pipeline
        }

        #region basic functions

        /// <summary>
        /// Get ProducerOptions.
        /// </summary>
        /// <param name="producerName"></param>
        /// <returns></returns>
        public ProducerOptions GetProducerOption(string producerName)
        {
            if (_rabbitConfig.Producers != null)
            {
                var connectionOption = _rabbitConfig.Producers.FirstOrDefault(o => o.ProducerName == producerName);

                if (connectionOption == null)
                {
                    throw new Exception($"Producer '{producerName}' not found!");
                }

                return connectionOption;
            }
            else
            {
                throw new Exception("No ProducerOptions added in RabbitHelper!");
            }
        }

        /// <summary>
        /// Get ConsumerOptions.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ConsumerOptions GetConsumerOption(string? consumerName)
        {
            if (_rabbitConfig.Consumers != null)
            {
                var connectionOption = _rabbitConfig.Consumers.FirstOrDefault(x => x.ConsumerName == consumerName);

                if (connectionOption == null)
                {
                    throw new Exception($"Consumer '{consumerName}' not found!");
                }

                return connectionOption;
            }
            else
            {
                throw new Exception("No ConsumerOptions added in RabbitHelper!");
            }
        }

        /// <summary>
        /// Publish any type of messages.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="producerName"></param>
        /// <param name="message"></param>
        public void Publish<T>(string producerName, T message, IBasicProperties? properties = null)
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

            _pipeline.Execute(token =>
            {
                try
                {
                    if (properties == null) properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;
                    _channel.BasicPublish(
                        exchange: option.ExchangeName, 
                        routingKey: option.RoutingKey, 
                        basicProperties: properties, 
                        body: body);

                    _logger?.LogInformation($"{producerName}|Published|{messageStr}");
                }
                catch (Exception e)
                {
                    _logger?.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Publish batch messages to queue(s)
        /// </summary>
        /// <param name="producerName"></param>
        /// <param name="messageList"></param>
        /// <param name="properties"></param>
        public void PublishBatch<T>(string producerName, IEnumerable<T?> messageList, IBasicProperties? properties = null)
        {
            var option = GetProducerOption(producerName);

            var messageObjs = messageList.ToList();

            _channel.ExchangeDeclare(option.ExchangeName, option.Type,
                durable: option.Durable, autoDelete: option.AutoDelete,
                arguments: option.Arguments);

            foreach (var messageObj in messageObjs)
            {
                var messageStr = JsonConvert.SerializeObject(messageObj);
                var body = Encoding.UTF8.GetBytes(messageStr);

                _pipeline.Execute(token =>
                {
                    try
                    {
                        if (properties == null) properties = _channel.CreateBasicProperties();
                        _channel.BasicPublish(
                            exchange: option.ExchangeName,
                            routingKey: option.RoutingKey,
                            basicProperties: properties,
                            body: body);
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
                        throw;
                    }
                });
            }
            _logger?.LogInformation($"{producerName}|Published a batch of messgages.");
        }

        /// <summary>
        /// Add a consumer by a custom consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <param name="onMessageReceived"></param>
        /// <param name="consumers"></param>
        public void AddConsumer(string consumerName, Action<string> onMessageReceived, int consumers = 1)
        {
            var option = GetConsumerOption(consumerName);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: option.PrefetchCount, global: false);

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

        /// <summary>
        /// Add an async consumer by a custom consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <param name="onMessageReceived"></param>
        /// <param name="consumers"></param>
        public void AddAsyncConsumer(string consumerName, Func<string, Task> onMessageReceivedAsync, int consumers = 1)
        {
            var option = GetConsumerOption(consumerName);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: option.PrefetchCount, global: false);

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

        #endregion

        #region utils

        /// <summary>
        /// Declare a queue based on RabbitMQ.Client.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            _channel.QueueDeclare(queue: queueName, durable, exclusive, autoDelete, arguments);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        /// <summary>
        /// Create a custom BasicProperties.
        /// </summary>
        /// <returns></returns>
        public IBasicProperties CreateBasicProperties()
        {
            return _channel.CreateBasicProperties();
        }

        #endregion

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