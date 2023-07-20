using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace Example.QuickStart
{
    public class BasicConsumer : RabbitConsumer<string>
    {
        public BasicConsumer(string connectionName, string consumerName, RabbitPool pool) : base(connectionName, consumerName, pool)
        {
        }

        protected override void MessageHandler(string message)
        {
            Console.WriteLine($"Receive: {message}");
        }
    }
}
