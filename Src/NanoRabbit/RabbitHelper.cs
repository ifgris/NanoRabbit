using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace NanoRabbit
{
    /// <summary>
    /// RabbitHelper
    /// </summary>
    public class RabbitHelper : IRabbitHelper, IDisposable
    {
        private readonly IConnection? _connection;
        private readonly ConcurrentDictionary<string, Task<IChannel>> _channels;
        private readonly RabbitConfiguration _rabbitConfig;
        private readonly ILogger _logger;

        /// <summary>
        /// RabbitHelper constructor.
        /// </summary>
        /// <param name="rabbitConfig"></param>
        /// <param name="logger"></param>
        /// <param name="connection"></param>
        public RabbitHelper(RabbitConfiguration rabbitConfig, ILogger logger, IConnection? connection)
        {
            _rabbitConfig = rabbitConfig;
            _logger = logger;
            _connection = connection;

            _channels = new ConcurrentDictionary<string, Task<IChannel>>();
        }

        // /// <summary>
        // /// Initiate Connections.
        // /// </summary>
        // /// <param name="rabbitConfig"></param>
        // /// <param name="logger"></param>
        // /// <returns></returns>
        // public static async Task<RabbitHelper> CreateAsync(RabbitConfiguration rabbitConfig, ILogger logger)
        // {
        //     ConnectionFactory factory = new();
        //     if (!string.IsNullOrEmpty(rabbitConfig.Uri))
        //     {
        //         factory.Uri = new Uri(rabbitConfig.Uri);
        //     }
        //     else
        //     {
        //         factory = new ConnectionFactory
        //         {
        //             HostName = rabbitConfig.HostName,
        //             Port = rabbitConfig.Port,
        //             VirtualHost = rabbitConfig.VirtualHost,
        //             UserName = rabbitConfig.UserName,
        //             Password = rabbitConfig.Password
        //         };
        //
        //         if (rabbitConfig.TLSConfig != null)
        //         {
        //             factory.Ssl.Enabled = rabbitConfig.TLSConfig.Enabled;
        //             factory.Ssl.ServerName = rabbitConfig.TLSConfig.ServerName;
        //             factory.Ssl.CertPath = rabbitConfig.TLSConfig.CertPath;
        //             factory.Ssl.CertPassphrase = rabbitConfig.TLSConfig.CertPassphrase;
        //             factory.Ssl.Version = rabbitConfig.TLSConfig.Version;
        //         }
        //     }
        //
        //     factory.ClientProvidedName = string.IsNullOrEmpty(rabbitConfig.ConnectionName)
        //         ? (!string.IsNullOrEmpty(rabbitConfig.UserName)
        //             ? $"nanorabbit:{rabbitConfig.UserName.ToLower()}"
        //             : "")
        //         : rabbitConfig.ConnectionName;
        //
        //
        //     IConnection connection;
        //     try
        //     {
        //         // wait for connection
        //         connection = await factory.CreateConnectionAsync();
        //         logger.LogInformation("RabbitMQ Connection established successfully.");
        //     }
        //     catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
        //     {
        //         logger.LogError(ex, "RabbitMQ Broker Unreachable. Failed to connect.");
        //         throw;
        //     }
        //     catch (TimeoutException ex)
        //     {
        //         logger.LogError(ex, "RabbitMQ Connection timed out.");
        //         throw;
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogError(ex, "An unexpected error occurred during RabbitMQ connection.");
        //         throw;
        //     }
        //
        //     // The RabbitHelper instance is created only when the connection is successfully established
        //     return new RabbitHelper(rabbitConfig, logger, connection);
        // }

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

                return connectionOption ?? throw new Exception($"Producer '{producerName}' not found!");
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

                return connectionOption ?? throw new Exception($"Consumer '{consumerName}' not found!");
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

            try
            {
                var option = GetProducerOption(producerName);

                var body = Encoding.UTF8.GetBytes(messageStr);

                await PublishMessageAsync(option, properties ?? new BasicProperties(), body);

                _logger.LogInformation($"{producerName}|Published|{messageStr}");
            }
            catch (Exception e)
            {
                _logger.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
                throw;
            }
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


            var option = GetProducerOption(producerName);
            var channel = await GetOrCreatePublishChannelAsync(option.ProducerName);

            await channel.ExchangeDeclareAsync(option.ExchangeName, option.Type,
                durable: option.Durable, autoDelete: option.AutoDelete,
                arguments: option.Arguments);

            var publishTasks = messageObjs.Select(async message =>
            {
                var messageStr = SerializeMessage(message) ?? "";
                var body = Encoding.UTF8.GetBytes(messageStr);


                try
                {
                    await PublishMessageAsync(option, properties ?? new BasicProperties(), body);
                }
                catch (Exception e)
                {
                    _logger.LogError($"{producerName}|Published|{messageStr}|Failed|{e.Message}");
                    throw;
                }
            });

            await Task.WhenAll(publishTasks);


            _logger.LogInformation($"{producerName}|Published a batch of messgages.");
        }

        #endregion

        #region utils

        public async Task<IChannel> GetChannelAsync(string channelName)
        {
            Task<IChannel> channelTask = _channels.GetOrAdd(channelName, async (key) =>
            {
                if (_connection == null)
                {
                    throw new Exception($"Connection is null when trying to create channel for producer: {key}");
                }

                try
                {
                    IChannel channel = await _connection.CreateChannelAsync();

                    // await channel.BasicQosAsync(0, GetConsumerOption(channelName).PrefetchCount, false);
                    return channel;
                }
                catch (Exception ex)
                {
                    _channels.TryRemove(key, out _);
                    throw new Exception($"Failed to create channel for producer '{key}': {ex.Message}", ex);
                }
            });

            return await channelTask;
        }

        public async Task ReleaseChannelAsync(string channelName)
        {
            if (_channels.TryRemove(channelName, out var channelTask))
            {
                var channel = channelTask.ConfigureAwait(false).GetAwaiter().GetResult();
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
            bool autoDelete = false, IDictionary<string, object?>? arguments = null)
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
            IDictionary<string, object?>? arguments)
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
            bool autoDelete = false, IDictionary<string, object?>? arguments = null)
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
            IDictionary<string, object?>? arguments = null)
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
        /// Publish message asynchronously.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="properties"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private async Task PublishMessageAsync(ProducerOptions option, BasicProperties properties, byte[] body)
        {
            var channel = await GetOrCreatePublishChannelAsync(option.ProducerName);

            await channel.BasicPublishAsync(
                exchange: option.ExchangeName,
                routingKey: option.RoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);
        }

        private async Task<IChannel> GetOrCreatePublishChannelAsync(string producerName)
        {
            Task<IChannel> channelCreationTask = _channels.GetOrAdd(producerName, async (key) =>
            {
                if (_connection == null)
                {
                    throw new Exception($"Connection is null when trying to create channel for producer: {key}");
                }

                try
                {
                    IChannel channel = await _connection.CreateChannelAsync();


                    return channel;
                }
                catch (Exception ex)
                {
                    _channels.TryRemove(key, out _);
                    throw new Exception($"Failed to create channel for producer '{key}': {ex.Message}", ex);
                }
            });

            return await channelCreationTask;
        }

        #endregion

        public void Dispose()
        {
            // Dispose Channels
            foreach (var channelTask in _channels.Values)
            {
                try
                {
                    var channel = channelTask.ConfigureAwait(false).GetAwaiter().GetResult();


                    if (channel.IsOpen)
                    {
                        channel.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during disposal of a channel task: {ex.Message}");
                    // channel?.Dispose();
                }
            }

            _channels.Clear();

            // Dispose Connection
            try
            {
                if (_connection.IsOpen)
                {
                    _connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                }

                _connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during disposal of connection: {ex.Message}");
            }
        }
    }
}