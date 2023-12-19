using NanoRabbit.Connection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace NanoRabbit.Producer;

/// <summary>
/// RabbitProducer, can be inherited by custom Producer.
/// </summary>
public class RabbitProducer
{
    private readonly IEnumerable<ProducerOptions> _producerOptionsList;

    public RabbitProducer(IEnumerable<ProducerOptions> producerOptionsList)
    {
        _producerOptionsList = producerOptionsList;
    }

    /// <summary>
    /// Publish message to queue(s)
    /// </summary>
    /// <param name="producerName"></param>
    /// <param name="message"></param>
    public void Publish(string producerName, string message)
    {
        var connectionOption = _producerOptionsList.FirstOrDefault(o => o.ProducerName == producerName);

        if (connectionOption == null)
        {
            // 没有找到指定名称的连接配置
            // 可根据实际情况处理，例如抛出异常或记录日志
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = connectionOption.HostName,
            Port = connectionOption.Port,
            UserName = connectionOption.UserName,
            Password = connectionOption.Password,
            VirtualHost = connectionOption.VirtualHost,
            AutomaticRecoveryEnabled = true
        };

        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(connectionOption.ExchangeName, connectionOption.Type, durable: connectionOption.Durable, autoDelete: connectionOption.AutoDelete, arguments: connectionOption.Arguments);
                var properties = channel.CreateBasicProperties();

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(
                    exchange: connectionOption.ExchangeName,
                    routingKey: connectionOption.RoutingKey,
                    basicProperties: properties,
                    body: body);
            }
        }
    }
    
    /// <summary>
    /// Publish message to queue(s)
    /// </summary>
    /// <param name="producerName"></param>
    /// <param name="message"></param>
    public void Publish<T>(string producerName, T message)
    {
        var connectionOption = _producerOptionsList.FirstOrDefault(o => o.ProducerName == producerName);

        if (connectionOption == null)
        {
            // 没有找到指定名称的连接配置
            // 可根据实际情况处理，例如抛出异常或记录日志
            return;
        }

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
                channel.ExchangeDeclare(connectionOption.ExchangeName, connectionOption.Type, durable: connectionOption.Durable, autoDelete: connectionOption.AutoDelete, arguments: connectionOption.Arguments);
                var properties = channel.CreateBasicProperties();

                var messageStr = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageStr);

                channel.BasicPublish(
                    exchange: connectionOption.ExchangeName,
                    routingKey: connectionOption.RoutingKey,
                    basicProperties: properties,
                    body: body);
            }
        }
    }
    
    /// <summary>
    /// Publish batch messages to queue(s)
    /// </summary>
    /// <param name="producerName"></param>
    /// <param name="messageList"></param>
    public void PublishBatch<T>(string producerName, IEnumerable<T> messageList)
    {
        var connectionOption = _producerOptionsList.FirstOrDefault(o => o.ProducerName == producerName);

        if (connectionOption == null)
        {
            // 没有找到指定名称的连接配置
            // 可根据实际情况处理，例如抛出异常或记录日志
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = connectionOption.HostName,
            Port = connectionOption.Port,
            UserName = connectionOption.UserName,
            Password = connectionOption.Password,
            VirtualHost = connectionOption.VirtualHost
        };

        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(connectionOption.ExchangeName, connectionOption.Type, durable: connectionOption.Durable, autoDelete: connectionOption.AutoDelete, arguments: connectionOption.Arguments);
                var properties = channel.CreateBasicProperties();

                foreach (var messageObj in messageList)
                {
                    var messageStr = JsonConvert.SerializeObject(messageObj);
                    var body = Encoding.UTF8.GetBytes(messageStr);
    
                    channel.BasicPublish(
                        exchange: connectionOption.ExchangeName,
                        routingKey: connectionOption.RoutingKey,
                        basicProperties: properties,
                        body: body);
                }
            }
        }
    }
}