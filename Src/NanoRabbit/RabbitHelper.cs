using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
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
        private IConnection? _connection;
        private readonly ConcurrentDictionary<string, IChannel> _channels;
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

            factory.ClientProvidedName = string.IsNullOrEmpty(_rabbitConfig.ConnectionName)
                ? (!string.IsNullOrEmpty(_rabbitConfig.UserName)
                    ? $"nanorabbit:{_rabbitConfig.UserName.ToLower()}"
                    : "")
                : _rabbitConfig.ConnectionName;

            _pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 }) // Add retry using the default options
                .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 seconds timeout
                .Build(); // Builds the resilience pipeline

            _pipeline.Execute(async _ =>
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync();
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
                {
                    _logger?.LogError($"RabbitMQ Unreachable, reconnecting...");
                    // throw;
                }
            });

            _channels = new ConcurrentDictionary<string, IChannel>();

            _asyncConsumers = new Dictionary<string, AsyncEventingBasicConsumer>();
            _logger = logger;
        }

        #region basic functions

        /// <summary>
        /// Get ProducerOptions.
        /// </summary>
        /// <param name="producerName"></param>
        /// <returns></returns>
        private ProducerOptions GetProducerOption(string producerName)
        {
            if (_rabbitConfig.Producers != null)
            {
                var connectionOption = _rabbitConfig.Producers.FirstOrDefault(o => o.ProducerName == producerName);

                return connectionOption == null
                    ? throw new Exception($"Producer '{producerName}' not found!")
                    : connectionOption;
            }

            throw new Exception("No ProducerOptions added in RabbitHelper!");
        }

        /// <summary>
        /// Get ConsumerOptions.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private ConsumerOptions GetConsumerOption(string? consumerName)
        {
            if (_rabbitConfig.Consumers != null)
            {
                var connectionOption = _rabbitConfig.Consumers.FirstOrDefault(x => x.ConsumerName == consumerName);

                return connectionOption == null
                    ? throw new Exception($"Consumer '{consumerName}' not found!")
                    : connectionOption;
            }

            throw new Exception("No ConsumerOptions added in RabbitHelper!");
        }

        /// <summary>
        /// Publish message asynchronously, extended from BasicPublish().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="producerName"></param>
        /// <param name="message"></param>
        /// <param name="properties"></param>
        public async Task PublishAsync<T>(string producerName, T message, BasicProperties? properties = null)
        {
            var messageStr = SerializeMessage(message) ?? "";

            await _pipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    var option = GetProducerOption(producerName);
                    var channel = await GetOrCreatePublishChannelAsync(option.ProducerName);

                    var body = Encoding.UTF8.GetBytes(messageStr);

                    await PublishMessageAsync(option, properties, body);

                    _logger.LogInformation($"{producerName}|Published|{messageStr}");
                }
                catch (Exception e)
                {
                    _logger.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
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
        public async Task PublishBatchAsync<T>(string producerName, IEnumerable<T?> messageList,
            BasicProperties? properties = null)
        {
            var messageObjs = messageList.ToList();

            await _pipeline.ExecuteAsync(async _ =>
            {
                var option = GetProducerOption(producerName);
                var channel = await GetOrCreatePublishChannelAsync(option.ProducerName);

                await channel.ExchangeDeclareAsync(option.ExchangeName, option.Type,
                    durable: option.Durable, autoDelete: option.AutoDelete,
                    arguments: option.Arguments);

                var publishTasks = messageObjs.Select(async message =>
                {
                    var messageStr = SerializeMessage(message) ?? "";
                    var body = Encoding.UTF8.GetBytes(messageStr);

                    await _pipeline.ExecuteAsync(async _ =>
                    {
                        try
                        {
                            await PublishMessageAsync(option, properties, body);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
                            throw;
                        }
                    });
                });

                await Task.WhenAll(publishTasks);
            });

            _logger.LogInformation($"{producerName}|Published a batch of messgages.");
        }

        /// <summary>
        /// Add an async consumer by a custom consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <param name="onMessageReceivedAsync"></param>
        /// <param name="consumers"></param>
        public void AddAsyncConsumer(string consumerName, Func<string, Task> onMessageReceivedAsync, int consumers = 1)
        {
            AddConsumerInternal(consumerName, onMessageReceivedAsync, null, consumers);
        }

        #endregion

        #region utils

        public IChannel GetChannel(string channelName)
        {
            return _channels.GetOrAdd(channelName, name =>
            {
                var channel = (_connection.CreateChannelAsync()).GetAwaiter().GetResult();
                (channel.BasicQosAsync(0, GetConsumerOption(name).PrefetchCount, false)).GetAwaiter().GetResult();
                return channel;
            });
        }

        public async Task ReleaseChannel(string channelName)
        {
            if (_channels.TryRemove(channelName, out var channel))
            {
                if (channel.IsOpen)
                    await channel.CloseAsync();
                await channel.DisposeAsync();
            }
        }

        /// <summary>
        /// Declare an exchange.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchangeName"></param>
        /// <param name="exchangeType"></param>
        /// <param name="durable"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public async Task ExchangeDeclareAsync(IChannel channel, string exchangeName, string exchangeType,
            bool durable = false,
            bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            await channel.ExchangeDeclareAsync(exchangeName, exchangeType, durable, autoDelete, arguments);
        }

        /// <summary>
        /// Bind an exchange to an exchange.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="routingKey"></param>
        /// <param name="arguments"></param>
        public async Task ExchangeBindAsync(IChannel channel, string destination, string source, string routingKey,
            IDictionary<string, object> arguments)
        {
            await channel.ExchangeBindAsync(destination, source, routingKey, arguments);
        }

        /// <summary>
        /// Delete an exchange.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchangeName"></param>
        /// <param name="ifUnused"></param>
        public async Task ExchangeDeleteAsync(IChannel channel, string exchangeName, bool ifUnused)
        {
            await channel.ExchangeDeleteAsync(exchangeName, ifUnused);
        }

        /// <summary>
        /// Declare a queue based on RabbitMQ.Client.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public async Task QueueDeclareAsync(IChannel channel, string queueName, bool durable = true,
            bool exclusive = false,
            bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            await channel.QueueDeclareAsync(queue: queueName, durable, exclusive, autoDelete, arguments);
        }

        /// <summary>
        /// Bind a queue to an exchange.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="arguments"></param>
        public async Task QueueBindAsync(IChannel channel, string queueName, string exchangeName, string routingKey,
            IDictionary<string, object>? arguments = null)
        {
            await channel.QueueBindAsync(queueName, exchangeName, routingKey, arguments);
        }

        /// <summary>
        /// Delete a queue.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="ifUnused"></param>
        /// <param name="ifEmpty"></param>
        public async Task QueueDeleteAsync(IChannel channel, string queueName, bool ifUnused, bool ifEmpty)
        {
            await channel.QueueDeleteAsync(queueName, ifUnused, ifEmpty);
        }

        /// <summary>
        /// Purge a queue of messages.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        public async Task QueuePurgeAsync(IChannel channel, string queueName)
        {
            await channel.QueuePurgeAsync(queueName);
        }

        /// <summary>
        /// Create a custom BasicProperties.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        [Obsolete]
        public IBasicProperties CreateBasicProperties(IChannel channel)
        {
            // return channel.CreateBasicProperties();
            return new BasicProperties();
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
            return typeof(T) == typeof(string)
                ? (message != null ? message.ToString() : "")
                : JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Set basic properties.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        [Obsolete]
        private IBasicProperties SetBasicProperties(IChannel channel, IBasicProperties? properties)
        {
            // properties ??= channel.CreateBasicProperties();
            // properties.Persistent = true;
            // return properties;
            return new BasicProperties();
        }

        /// <summary>
        /// Publish message asynchronously.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="properties"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private async Task PublishMessageAsync(ProducerOptions option, BasicProperties properties, byte[] body)
        {
            var channel = await GetOrCreatePublishChannelAsync(option.ProducerName);
            if (channel != null)
            {
                await channel.BasicPublishAsync(
                    exchange: option.ExchangeName,
                    routingKey: option.RoutingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);
            }
        }

        /// <summary>
        /// Add a consumer (sync or async) by a custom consumerName.
        /// </summary>
        /// <param name="consumerName"></param>
        /// <param name="onMessageReceivedAsync"></param>
        /// <param name="onMessageReceived"></param>
        /// <param name="consumers"></param>
        private async Task AddConsumerInternal(string consumerName, Func<string, Task>? onMessageReceivedAsync,
            Action<string>? onMessageReceived = null, int consumers = 1)
        {
            var option = GetConsumerOption(consumerName);


            for (int i = 0; i < consumers; i++)
            {
                var consumerId = string.Concat(option.QueueName, "-", i + 1);
                IChannel? channel = await _connection?.CreateChannelAsync();
                await channel?.BasicQosAsync(prefetchSize: 0, prefetchCount: option.PrefetchCount, global: false);
                if (channel != null)
                {
                    _channels.TryAdd(consumerId, channel);

                    if (!_asyncConsumers.ContainsKey(consumerId))
                    {
                        var consumer = new AsyncEventingBasicConsumer(channel);
                        consumer.ReceivedAsync += async (_, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            if (onMessageReceivedAsync != null)
                                await onMessageReceivedAsync(message);

                            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                            await Task.Yield();
                        };

                        await channel.BasicConsumeAsync(queue: option.QueueName, autoAck: false, consumer: consumer);
                        _asyncConsumers[consumerId] = consumer;
                    }
                }
            }
        }

        private async Task<IChannel> GetOrCreatePublishChannelAsync(string producerName)
        {
            return _channels.GetOrAdd(producerName, _ =>
            {
                IChannel? channel = (_connection?.CreateChannelAsync()).GetAwaiter().GetResult();
                if (channel != null)
                {
                    return channel;
                }
                else
                {
                    throw new Exception($"Could not create channel: {producerName}");
                }
            });
        }

        #endregion

        public void Dispose()
        {
            foreach (var channel in _channels.Values)
            {
                if (channel.IsOpen) (channel.CloseAsync()).ConfigureAwait(false).GetAwaiter().GetResult();
                channel.Dispose();
            }

            if (_connection != null && _connection.IsOpen) (_connection.CloseAsync()).GetAwaiter().GetResult();
            _connection?.Dispose();
        }
    }
}