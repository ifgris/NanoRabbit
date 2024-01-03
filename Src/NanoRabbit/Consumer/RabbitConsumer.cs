using NanoRabbit.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NanoRabbit.Consumer;

public interface IRabbitConsumer
{
    public ConsumerOptions GetMe(string consumerName);

    public void Receive(
        string consumerName,
        Action<string> messageHandler,
        uint prefetchSize = 0,
        ushort prefetchCount = 0,
        bool qosGlobal = false
    );

    public Task ReceiveAsync(
        string consumerName,
        Action<string> messageHandler,
        uint prefetchSize = 0,
        ushort prefetchCount = 0,
        bool qosGlobal = false
    );
}

/// <summary>
/// RabbitConsumer, can be inherited by custom Consumer
/// </summary>
public class RabbitConsumer : IRabbitConsumer
{
    private readonly ILogger<RabbitConsumer>? _logger;
    private readonly IEnumerable<ConsumerOptions> _consumerOptionsList;

    public delegate void MessageHandler(string message);

    public RabbitConsumer(IEnumerable<ConsumerOptions> consumerOptionsList, ILogger<RabbitConsumer>? logger = null)
    {
        _consumerOptionsList = consumerOptionsList;
        _logger = logger;
    }

    /// <summary>
    /// Get ConsumerOptions.
    /// </summary>
    /// <param name="consumerName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public ConsumerOptions GetMe(string consumerName)
    {
        var connectionOption = _consumerOptionsList.FirstOrDefault(x => x.ConsumerName == consumerName);

        if (connectionOption == null)
        {
            throw new Exception($"Consumer: {consumerName} not found!");
        }

        return connectionOption;
    }

    /// <summary>
    /// Receive messages from queue
    /// </summary>
    /// <param name="consumerName">Name of consumer</param>
    /// <param name="messageHandler"></param>
    /// <param name="prefetchSize">BasicQos prefetchSize</param>
    /// <param name="prefetchCount">BasicQos prefetchCount</param>
    /// <param name="qosGlobal">BasicQos global</param>
    public void Receive(
        string consumerName,
        Action<string> messageHandler,
        uint prefetchSize = 0,
        ushort prefetchCount = 0,
        bool qosGlobal = false
    )
    {
        var connectionOption = GetMe(consumerName);

        try
        {
            Task.Run(() =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = connectionOption.HostName,
                    Port = connectionOption.Port,
                    UserName = connectionOption.UserName,
                    Password = connectionOption.Password,
                    VirtualHost = connectionOption.VirtualHost,
                    AutomaticRecoveryEnabled = connectionOption.AutomaticRecoveryEnabled
                };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.BasicQos(prefetchSize, prefetchCount, qosGlobal);
                        var consumer = new EventingBasicConsumer(channel);

                        consumer.Received += (_, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            try
                            {
                                // handle incoming message
                                _logger?.LogDebug($"Received message: {message}");
                                messageHandler(message);
                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                            catch (Exception e)
                            {
                                _logger?.LogError(e, e.Message);
                            }
                        };

                        channel.BasicConsume(
                            queue: connectionOption.QueueName,
                            autoAck: false,
                            consumer: consumer);

                        Task.Delay(1000).Wait();
                    }
                }
            });
        }
        catch (Exception e)
        {
            _logger?.LogError(e, e.Message);
        }
    }

    /// <summary>
    /// Receive messages from queue
    /// </summary>
    /// <param name="consumerName">Name of consumer</param>
    /// <param name="messageHandler"></param>
    /// <param name="prefetchSize">BasicQos prefetchSize</param>
    /// <param name="prefetchCount">BasicQos prefetchCount</param>
    /// <param name="qosGlobal">BasicQos global</param>
    public async Task ReceiveAsync(
        string consumerName,
        Action<string> messageHandler,
        uint prefetchSize = 0,
        ushort prefetchCount = 0,
        bool qosGlobal = false
    )
    {
        var connectionOption = _consumerOptionsList.FirstOrDefault(x => x.ConsumerName == consumerName);

        if (connectionOption == null)
        {
            throw new Exception($"Consumer: {consumerName} not found!");
        }

        var tcs = new TaskCompletionSource<object>();

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = connectionOption.HostName,
                Port = connectionOption.Port,
                UserName = connectionOption.UserName,
                Password = connectionOption.Password,
                VirtualHost = connectionOption.VirtualHost,
                AutomaticRecoveryEnabled = connectionOption.AutomaticRecoveryEnabled
            };

            // use async-oriented consumer dispatcher. Only compatible with IAsyncBasicConsumer implementations
            factory.DispatchConsumersAsync = true;

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.BasicQos(prefetchSize, prefetchCount, qosGlobal);
                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.Received += async (_, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        try
                        {
                            // handle incoming message
                            _logger?.LogDebug($"Received message: {message}");
                            messageHandler(message);
                            channel.BasicAck(ea.DeliveryTag, false);
                            await Task.Yield();
                        }
                        catch (Exception e)
                        {
                            _logger?.LogError(e, e.Message);
                        }
                    };

                    channel.BasicConsume(
                        queue: connectionOption.QueueName,
                        autoAck: false,
                        consumer: consumer);

                    // wait for message
                    await tcs.Task; // await for the TaskCompletionSource to be signaled
                }
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, e.Message);
        }
    }
}