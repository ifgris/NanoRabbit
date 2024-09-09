using RabbitMQ.Client;
using System.Threading.Channels;

namespace NanoRabbit;

/// <summary>
/// RabbitHelper interface
/// </summary>
public interface IRabbitHelper
{
    #region basic functions

    /// <summary>
    /// Publish message, extended from BasicPublish().
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="producerName"></param>
    /// <param name="message"></param>
    /// <param name="properties"></param>
    public void Publish<T>(string producerName, T message, IBasicProperties? properties = null);
    /// <summary>
    /// Publish a batch of messages, extended from BasicPublish().
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="producerName"></param>
    /// <param name="messageList"></param>
    /// <param name="properties"></param>
    public void PublishBatch<T>(string producerName, IEnumerable<T?> messageList, IBasicProperties? properties = null);
    /// <summary>
    /// Add a consumer by using predefined consumer configs.
    /// </summary>
    /// <param name="consumerName"></param>
    /// <param name="onMessageReceived"></param>
    /// <param name="consumers"></param>
    public void AddConsumer(string consumerName, Action<string> onMessageReceived, int consumers = 1);
    /// <summary>
    /// Add a asynchronous consumer by using predefined consumer configs.
    /// </summary>
    /// <param name="consumerName"></param>
    /// <param name="onMessageReceivedAsync"></param>
    /// <param name="consumers"></param>
    public void AddAsyncConsumer(string consumerName, Func<string, Task> onMessageReceivedAsync, int consumers = 1);

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
    public void ExchangeDeclare(string exchangeName, string exchangeType, bool durable = false, bool autoDelete = false, IDictionary<string, object>? arguments = null);
    /// <summary>
    /// Declare a queue.
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="durable"></param>
    /// <param name="exclusive"></param>
    /// <param name="autoDelete"></param>
    /// <param name="arguments"></param>
    public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null);
    /// <summary>
    /// Bind a queue to an exchange.
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="exchangeName"></param>
    /// <param name="routingKey"></param>
    /// <param name="arguments"></param>
    public void QueueBind(string queueName, string exchangeName, string routingKey, IDictionary<string, object> arguments);
    /// <summary>
    /// Delete a queue.
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="ifUnused"></param>
    /// <param name="ifEmpty"></param>
    public void QueueDelete(string queueName, bool ifUnused, bool ifEmpty);
    /// <summary>
    /// Purge a queue of messages.
    /// </summary>
    /// <param name="queueName"></param>
    public void QueuePurge(string queueName);
    /// <summary>
    /// Create a custom BasicProperties.
    /// </summary>
    /// <returns></returns>
    public IBasicProperties CreateBasicProperties();

    #endregion
}