namespace NanoRabbit
{
    /// <summary>
    /// Async message handler interface.
    /// </summary>
    public interface IAsyncMessageHandler
    {
        /// <summary>
        /// Override this method to handle message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task HandleMessageAsync(string message);
    }

    /// <summary>
    /// Default async message handler.
    /// </summary>
    public class DefaultAsyncMessageHandler : IAsyncMessageHandler
    {
        public virtual async Task HandleMessageAsync(string message)
        {
            // Default processing code
            Console.WriteLine($"[x] Received: {message}");
            // Simulate work
            await Task.Delay(1000);
            Console.WriteLine("[x] Done");
        }
    }
}
