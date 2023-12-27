using NanoRabbit.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NanoRabbit.Consumer;

public interface IRabbitConsumer
{
    void Receive(
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
    private readonly IEnumerable<ConsumerOptions> _consumerOptionsList;

    public delegate void MessageHandler(string message);

    public RabbitConsumer(IEnumerable<ConsumerOptions> consumerOptionsList)
    {
        _consumerOptionsList = consumerOptionsList;
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
        var connectionOption = _consumerOptionsList.FirstOrDefault(x => x.ConsumerName == consumerName);

        if (connectionOption == null)
        {
            throw new Exception($"Consumer: {consumerName} not found!");
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
                // SocketFactory = null,
                // AmqpUriSslProtocols = SslProtocols.None,
                // AuthMechanisms = null,
                // DispatchConsumersAsync = false,
                // ConsumerDispatchConcurrency = 0,
                // NetworkRecoveryInterval = default,
                // MemoryPool = null,
                // HandshakeContinuationTimeout = default,
                // ContinuationTimeout = default,
                // EndpointResolverFactory = null,
                // RequestedConnectionTimeout = default,
                // SocketReadTimeout = default,
                // SocketWriteTimeout = default,
                // Ssl = null,
                // TopologyRecoveryEnabled = false,
                // TopologyRecoveryFilter = null,
                // TopologyRecoveryExceptionHandler = null,
                // Endpoint = null,
                // ClientProperties = null,
                // CredentialsProvider = null,
                // CredentialsRefresher = null,
                // RequestedChannelMax = 0,
                // RequestedFrameMax = 0,
                // RequestedHeartbeat = default,
                // MaxMessageSize = 0,
                // Uri = null,
                // ClientProvidedName = null
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // TODO: will move to params
                    channel.BasicQos(prefetchSize, prefetchCount, qosGlobal);
                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (_, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        try
                        {
                            // handle incoming message
                            messageHandler(message);
                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };

                    channel.BasicConsume(
                        queue: connectionOption.QueueName,
                        autoAck: false,
                        consumer: consumer);

                    // wait for message
                    while (true)
                    {
                        Task.Delay(1000).Wait();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // throw;
        }
    }
}