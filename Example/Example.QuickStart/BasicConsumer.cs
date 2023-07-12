using NanoRabbit.NanoRabbit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
