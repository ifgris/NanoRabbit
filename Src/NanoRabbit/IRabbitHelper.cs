namespace NanoRabbit;

public interface IRabbitHelper
{
    public void Publish<T>(string producerName, T message);
    public void PublishBatch<T>(string producerName, IEnumerable<T?> messageList);
    public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null);
    public void AddConsumer(string consumerName, Action<string> onMessageReceived, int consumers = 1);
    public void AddAsyncConsumer(string consumerName, Func<string, Task> onMessageReceivedAsync, int consumers = 1);
}