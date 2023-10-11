namespace NanoRabbit.Consumer;

/// <summary>
/// IRabbitConsumer
/// </summary>
public interface IRabbitConsumer : IDisposable
{
    /// <summary>
    /// Receive from Queue.
    /// </summary>
    void Receive();
}