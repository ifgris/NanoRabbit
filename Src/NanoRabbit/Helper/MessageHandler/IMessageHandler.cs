namespace NanoRabbit.Helper.MessageHandler
{
    public interface IMessageHandler
    {
        void HandleMessage(string message);
    }

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

    public class ForwardMessageHandler : IMessageHandler
    {
        public virtual void HandleMessage(string message)
        {
            // todo
        }
    }
}
