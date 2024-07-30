namespace NanoRabbit;

/// <summary>
/// Message handler interface.
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Override this method to handle message.
    /// </summary>
    /// <param name="message"></param>
    void HandleMessage(string message);
}

/// <summary>
/// Default message handler.
/// </summary>
public class DefaultMessageHandler : IMessageHandler
{
    public virtual void HandleMessage(string message)
    {
        // Default processing code
        Console.WriteLine($"[x] Received: {message}");
        // Simulate work
        Task.Delay(1000).Wait();
        Console.WriteLine("[x] Done");
    }
}

/// <summary>
/// Forwarding message handler.
/// </summary>
public class ForwardMessageHandler : IMessageHandler
{
    public virtual void HandleMessage(string message)
    {
        // todo
    }
}