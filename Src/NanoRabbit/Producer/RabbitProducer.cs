using System.Collections.Concurrent;
using NanoRabbit.Connection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using NanoRabbit.Model;

namespace NanoRabbit.Producer;

/// <summary>
/// RabbitProducer, can be inherited by custom Producer.
/// </summary>
public class RabbitProducer
{
    private readonly IEnumerable<ProducerOptions> _producerOptionsList;
    private readonly ConcurrentDictionary<string, ResendMsgModel> _resendMsgDic = new();

    public RabbitProducer(IEnumerable<ProducerOptions> producerOptionsList)
    {
        _producerOptionsList = producerOptionsList;
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
            throw new Exception($"Producer: {producerName} not found!");
        }

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

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(connectionOption.ExchangeName, connectionOption.Type,
                        durable: connectionOption.Durable, autoDelete: connectionOption.AutoDelete,
                        arguments: connectionOption.Arguments);
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
            
            // resend cached messages
            if (connectionOption.AutomaticResend)
            {
                ResendCachedMessage(producerName);
            }
        }
        catch (Exception e)
        {
            if (TryAddResendMessage(producerName, message))
            {
            }
            else
            {
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
            throw new Exception($"Producer: {producerName} not found!");
        }

        var messageObjs = messageList.ToList();
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

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(connectionOption.ExchangeName, connectionOption.Type,
                        durable: connectionOption.Durable, autoDelete: connectionOption.AutoDelete,
                        arguments: connectionOption.Arguments);
                    var properties = channel.CreateBasicProperties();

                    foreach (var messageObj in messageObjs)
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

            // resend cached messages
            if (connectionOption.AutomaticResend)
            {
                ResendCachedMessage(producerName);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            foreach (var message in messageObjs)
            {
                if (TryAddResendMessage(producerName, message))
                {
                }
                else
                {
                }
            }
        }
    }

    /// <summary>
    /// Try add resend message to concurrent dictionary
    /// </summary>
    /// <param name="producerName"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private bool TryAddResendMessage<T>(string producerName, T message)
    {
        bool tryFlag = false;

        if (_resendMsgDic.TryGetValue(producerName, out var dicResult))
        {
            if (dicResult.MessageList != null)
            {
                if (message != null)
                {
                    dicResult.MessageList.Enqueue(new MsgInfoModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        GenerateTime = DateTime.Now,
                        Message = message
                    });
                    tryFlag = true;
                }
            }
        }
        else
        {
            // Create message list if doesn't exists
            var resendList = new ResendMsgModel
            {
                MessageList = new ConcurrentQueue<MsgInfoModel>()
            };
            resendList.MessageList.Enqueue(new MsgInfoModel
            {
                Id = Guid.NewGuid().ToString(),
                GenerateTime = DateTime.Now,
                Message = message
            });
            tryFlag = _resendMsgDic.TryAdd(producerName, resendList);
        }

        return tryFlag;
    }

    /// <summary>
    /// Resend the cached messages
    /// </summary>
    /// <param name="producerName"></param>
    private void ResendCachedMessage(string producerName)
    {
        if (_resendMsgDic.Any())
        {
            if (_resendMsgDic.TryGetValue(producerName, out var resultDic))
            {
                if (resultDic.MessageList != null)
                {
                    while (resultDic.MessageList.TryDequeue(out var item))
                    {
                        Publish<dynamic>(producerName, item);
                    }
                }
            }
        }
    }
}