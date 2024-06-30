using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoRabbit.Helper.MessageHandler
{
    public interface IAsyncMessageHandler
    {
        Task HandleMessageAsync(string message);
    }

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
