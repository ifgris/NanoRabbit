using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.Consumer;

/// <summary>
/// RabbitConsumer, can be inherited by custom Consumer
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class RabbitConsumer<T> : IRabbitConsumer<T>
{
    private readonly IModel _channel;
    private readonly ConsumerConfig _consumerConfig;
    private readonly ILogger<RabbitConsumer<T>>? _logger;

    protected RabbitConsumer(string connectionName, string consumerName, IRabbitPool rabbitPool,
        ILogger<RabbitConsumer<T>>? logger)
    {
        var pool = rabbitPool;
        _channel = pool.GetConnection(connectionName).CreateModel();
        _consumerConfig = pool.GetConsumerConfig(consumerName);
        _logger = logger;
        if (RabbitPoolExtensions.GlobalConfig != null && !RabbitPoolExtensions.GlobalConfig.EnableLogging)
        {
            _logger = null;
        }
    }

    protected RabbitConsumer(string queueName, IRabbitPool rabbitPool, ILogger<RabbitConsumer<T>>? logger)
    {
        var pool = rabbitPool;
        (string connectionName, string consumerConfigName) = pool.GetConfigsByQueueName(queueName);
        _channel = pool.GetConnection(connectionName).CreateModel();
        _consumerConfig = pool.GetConsumerConfig(consumerConfigName);
        _logger = logger;
        if (RabbitPoolExtensions.GlobalConfig != null && !RabbitPoolExtensions.GlobalConfig.EnableLogging)
        {
            _logger = null;
        }
    }

    /// <summary>
    /// ConsumeTask runs in ConsumeThread.
    /// </summary>
    private void ConsumeTask()
    {
        var consumer = new EventingBasicConsumer(_channel);
        while (true)
        {
            try
            {
                consumer.Received += (model, ea) =>
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var receiveObj = JsonConvert.DeserializeObject<T>(body);
                    if (receiveObj != null)
                    {
                        MessageHandler(receiveObj);
                    }
                };
                _channel.BasicConsume(queue: _consumerConfig.QueueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
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
            _logger?.LogError(ex, ex.Message);
        }
    }

    /// <summary>
    /// Handle with the received message.
    /// </summary>
    /// <param name="message"></param>
    public abstract void MessageHandler(T message);

    /// <summary>
    /// Start consumer thread method
    /// </summary>
    public void StartConsuming()
    {
        Task.Run(ConsumeTask);
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        _channel.Dispose();
    }
}