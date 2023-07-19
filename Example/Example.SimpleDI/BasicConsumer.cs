﻿using NanoRabbit.Connection;
using NanoRabbit.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.SimpleDI
{
    public class BasicConsumer : RabbitConsumer<string>
    {
        public BasicConsumer(string connectionName, string consumerName, IRabbitPool pool) : base(connectionName, consumerName, pool)
        {
        }

        protected override void MessageHandler(string message)
        {
            Console.WriteLine($"Receive: {message}");
        }
    }
}