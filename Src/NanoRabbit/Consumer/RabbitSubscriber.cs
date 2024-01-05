using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NanoRabbit.Consumer;

public class RabbitSubscriber : IHostedService
{
    private readonly ILogger<RabbitSubscriber>? _logger;
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly IRabbitConsumer _consumer;
    private readonly string _consumerName;


    public RabbitSubscriber(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger)
    {
        _consumer = consumer;
        _consumerName = consumerName;
        _logger = logger;
        try
        {
            var consumerOptions = consumer.GetMe(consumerName);
            var factory = new ConnectionFactory
            {
                HostName = consumerOptions.HostName,
                Port = consumerOptions.Port,
                UserName = consumerOptions.UserName,
                Password = consumerOptions.Password,
                VirtualHost = consumerOptions.VirtualHost,
                AutomaticRecoveryEnabled = consumerOptions.AutomaticRecoveryEnabled
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (Exception ex)
        {
            _logger?.LogError($"RabbitSubscriber init error,ex:{ex.Message}");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Register(_consumerName);
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _connection?.Close();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle messages
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual bool HandleMessage(string message)
    {
        throw new NotImplementedException();
    }

    // Register a consumer
    public void Register(string consumerName)
    {
        var consumerOptions = _consumer.GetMe(consumerName);
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var result = HandleMessage(message);
            if (result)
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        };
        _channel.BasicConsume(queue: consumerOptions.QueueName, consumer: consumer, autoAck: false);
    }
    

}