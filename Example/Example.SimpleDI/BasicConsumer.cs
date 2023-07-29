﻿using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace Example.SimpleDI
{
    public class BasicConsumer : RabbitConsumer<string>
    {
        private readonly ILogger _logger;

        public BasicConsumer(string connectionName, string consumerName, IRabbitPool pool, ILogger<RabbitConsumer<string>> logger) : base(connectionName, consumerName, pool, logger)
        {
            _logger = logger;
        }

        public override void MessageHandler(string message)
        {
            _logger.LogInformation($"ConsumerLogging: Receive: {message}");
        }
    }
}