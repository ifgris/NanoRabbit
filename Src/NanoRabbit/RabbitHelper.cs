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

                // TODO needs testing.
                if (_rabbitConfig.TLSConfig != null)
                {
                    factory.Ssl.Enabled = _rabbitConfig.TLSConfig.Enabled;
                    factory.Ssl.ServerName = _rabbitConfig.TLSConfig.ServerName;
                    factory.Ssl.CertPath = _rabbitConfig.TLSConfig.CertPath;
                    factory.Ssl.CertPassphrase = _rabbitConfig.TLSConfig.CertPassphrase;
                    factory.Ssl.Version = _rabbitConfig.TLSConfig.Version;
                }
            }

            factory.ClientProvidedName = string.IsNullOrEmpty(_rabbitConfig.ConnectionName) ?
                (!string.IsNullOrEmpty(_rabbitConfig.UserName) ? $"nanorabbit:{_rabbitConfig.UserName.ToLower()}" : "") : _rabbitConfig.ConnectionName;

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

                return connectionOption == null ? throw new Exception($"Producer '{producerName}' not found!") : connectionOption;
            }

            throw new Exception("No ProducerOptions added in RabbitHelper!");
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

                return connectionOption == null ? throw new Exception($"Consumer '{consumerName}' not found!") : connectionOption;
            }

            throw new Exception("No ConsumerOptions added in RabbitHelper!");
        }

        /// <summary>
        /// Publish message, extended from BasicPublish().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="producerName"></param>
        /// <param name="message"></param>
        public void Publish<T>(string producerName, T message, IBasicProperties? properties = null)
        {
            var option = GetProducerOption(producerName);

            var messageStr = SerializeMessage(message) ?? "";

            var body = Encoding.UTF8.GetBytes(messageStr);

            _pipeline.Execute(token =>
            {
                try
                {
                    properties = SetBasicProperties(properties);
                    PublishMessage(option, properties, body);

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
        /// Publish a batch of messages, extended from BasicPublish().
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

            messageObjs.ForEach(message =>
            {
                var messageStr = SerializeMessage(message) ?? "";
                var body = Encoding.UTF8.GetBytes(messageStr);

                _pipeline.Execute(token =>
                {
                    try
                    {
                        properties = SetBasicProperties(properties);
                        PublishMessage(option, properties, body);
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
                        throw;
                    }
                });
            });

            _logger?.LogInformation($"{producerName}|Published a batch of messgages.");
        }

        /// <summary>
        /// Publish message asynchronously, extended from BasicPublish().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="producerName"></param>
        /// <param name="message"></param>
        /// <param name="properties"></param>
        public async Task PublishAsync<T>(string producerName, T message, IBasicProperties? properties = null)
        {
            var option = GetProducerOption(producerName);

            var messageStr = SerializeMessage(message) ?? "";

            var body = Encoding.UTF8.GetBytes(messageStr);

            await _pipeline.ExecuteAsync(async token =>
            {
                try
                {
                    properties = SetBasicProperties(properties);
                    await PublishMessageAsync(option, properties, body);

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
        /// Publish a batch of messages asynchronously, extended from BasicPublish().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="producerName"></param>
        /// <param name="messageList"></param>
        /// <param name="properties"></param>
        public async Task PublishBatchAsync<T>(string producerName, IEnumerable<T?> messageList, IBasicProperties? properties = null)
        {
            var option = GetProducerOption(producerName);

            var messageObjs = messageList.ToList();

            _channel.ExchangeDeclare(option.ExchangeName, option.Type,
                durable: option.Durable, autoDelete: option.AutoDelete,
                arguments: option.Arguments);

            var publishTasks = messageObjs.Select(async message =>
            {
                var messageStr = SerializeMessage(message) ?? "";
                var body = Encoding.UTF8.GetBytes(messageStr);

                await _pipeline.ExecuteAsync(async token =>
                {
                    try
                    {
                        properties = SetBasicProperties(properties);
                        await PublishMessageAsync(option, properties, body);
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
                        throw;
                    }
                });
            });

            await Task.WhenAll(publishTasks);

            _logger?.LogInformation($"{producerName}|Published a batch of messgages.");

        }

        /// <summary>
        /// Add a sync consumer by a custom consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <param name="onMessageReceived"></param>
        /// <param name="consumers"></param>
        public void AddConsumer(string consumerName, Action<string> onMessageReceived, int consumers = 1)
        {
            AddConsumerInternal(consumerName, null, onMessageReceived, consumers, isAsync: false);
        }

        /// <summary>
        /// Add an async consumer by a custom consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <param name="onMessageReceivedAsync"></param>
        /// <param name="consumers"></param>
        public void AddAsyncConsumer(string consumerName, Func<string, Task> onMessageReceivedAsync, int consumers = 1)
        {
            AddConsumerInternal(consumerName, onMessageReceivedAsync, null, consumers, isAsync: true);
        }

        #endregion

        #region utils

        /// <summary>
        /// Declare an exchange.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="exchangeType"></param>
        /// <param name="durable"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public void ExchangeDeclare(string exchangeName, string exchangeType, bool durable = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            _channel.ExchangeDeclare(exchangeName, exchangeType, durable, autoDelete, arguments);
        }

        /// <summary>
        /// Bind an exchange to an exchange.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="source"></param>
        /// <param name="routingKey"></param>
        /// <param name="arguments"></param>
        public void ExchangeBind(string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.ExchangeBind(destination, source, routingKey, arguments);
        }
        
        /// <summary>
        /// Delete an exchange.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="ifUnused"></param>
        public void ExchangeDelete(string exchangeName, bool ifUnused)
        {
            _channel.ExchangeDelete(exchangeName, ifUnused);
        }

        /// <summary>
        /// Declare a queue based on RabbitMQ.Client.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public void QueueDeclare(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            _channel.QueueDeclare(queue: queueName, durable, exclusive, autoDelete, arguments);
        }

        /// <summary>
        /// Bind a queue to an exchange.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="arguments"></param>
        public void QueueBind(string queueName, string exchangeName, string routingKey, IDictionary<string, object>? arguments = null)
        {
            _channel.QueueBind(queueName, exchangeName, routingKey, arguments);
        }

        /// <summary>
        /// Delete a queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="ifUnused"></param>
        /// <param name="ifEmpty"></param>
        public void QueueDelete(string queueName, bool ifUnused, bool ifEmpty)
        {
            _channel.QueueDelete(queueName, ifUnused, ifEmpty);
        }

        /// <summary>
        /// Purge a queue of messages.
        /// </summary>
        /// <param name="queueName"></param>
        public void QueuePurge(string queueName)
        {
            _channel.QueuePurge(queueName);
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

        #region private functions

        /// <summary>
        /// Serialize message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        private string? SerializeMessage<T>(T message)
        {
            return typeof(T) == typeof(string) ? (message != null ? message.ToString() : "") : JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Set basic properties.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        private IBasicProperties SetBasicProperties(IBasicProperties? properties)
        {
            properties ??= _channel.CreateBasicProperties();
            properties.Persistent = true;
            return properties;
        }

        /// <summary>
        /// Publish message.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="properties"></param>
        /// <param name="body"></param>
        private void PublishMessage(ProducerOptions option, IBasicProperties properties, byte[] body)
        {
            _channel.BasicPublish(
                exchange: option.ExchangeName,
                routingKey: option.RoutingKey,
                basicProperties: properties,
                body: body);
        }

        /// <summary>
        /// Publish message asynchronously.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="properties"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private async Task PublishMessageAsync(ProducerOptions option, IBasicProperties properties, byte[] body)
        {
            await Task.Run(() =>
            {
                _channel.BasicPublish(
                    exchange: option.ExchangeName,
                    routingKey: option.RoutingKey,
                    basicProperties: properties,
                    body: body);
            });
        }

        /// <summary>
        /// Add a consumer (sync or async) by a custom consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <param name="onMessageReceived"></param>
        /// <param name="consumers"></param>
        /// <param name="isAsync"></param>
        private void AddConsumerInternal(string consumerName, Func<string, Task>? onMessageReceivedAsync, Action<string>? onMessageReceived = null, int consumers = 1, bool isAsync = false)
        {
            var option = GetConsumerOption(consumerName);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: option.PrefetchCount, global: false);

            for (int i = 0; i < consumers; i++)
            {
                var consumerId = string.Concat(option.QueueName, "-", i + 1);

                if (isAsync && !_asyncConsumers.ContainsKey(consumerId))
                {
                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        if (onMessageReceivedAsync != null)
                            await onMessageReceivedAsync(message);

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        await Task.Yield();
                    };

                    _channel.BasicConsume(queue: option.QueueName, autoAck: false, consumer: consumer);
                    _asyncConsumers[consumerId] = consumer;
                }
                else if (!isAsync && !_consumers.ContainsKey(consumerId))
                {
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        if (onMessageReceived != null)
                            onMessageReceived(message);

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };

                    _channel.BasicConsume(queue: option.QueueName, autoAck: false, consumer: consumer);
                    _consumers[consumerId] = consumer;
                }
            }
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