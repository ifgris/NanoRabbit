using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace Example.SimpleDI
{
    public class BasicConsumer : RabbitConsumer<string>
    {
        public BasicConsumer(string connectionName, string consumerName, IRabbitPool pool) : base(connectionName, consumerName, pool)
        {
        }

        public override void MessageHandler(string message)
        {
            Console.WriteLine($"Receive: {message}");
        }
    }
}
