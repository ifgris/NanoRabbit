using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;

namespace NanoRabbit.Connection
{
    /// <summary>
    /// Connection options.
    /// </summary>
    public class ConnectOptions
    {
        public ConnectOptions([Required] string connectionName)
        {
            ConnectionName = connectionName;
        }

        public string ConnectionName { get; set; }
        public ConnectConfig? ConnectConfig { get; set; }
        public ConnectUri? ConnectUri { get; set; }
        public List<ProducerConfig>? ProducerConfigs { get; set; }
        public List<ConsumerConfig>? ConsumerConfigs { get; set; }
    }

    /// <summary>
    /// Conenction configurations
    /// </summary>
    public class ConnectConfig
    {
        /// <summary>
        /// RabbitMQ HostName, default: localhost
        /// </summary>
        public string HostName { get; set; } = "localhost";
        /// <summary>
        /// RabbitMQ AmqpTcpEndpoint port, default: 5672
        /// </summary>
        public int Port { get; set; } = 5672;
        /// <summary>
        /// RabbitMQ UserName, default: guest
        /// </summary>
        public string UserName { get; set; } = "guest";
        /// <summary>
        /// RabbitMQ Password, default: guest
        /// </summary>
        public string Password { get; set; } = "guest";
        /// <summary>
        /// RabbitMQ VirtualHost, default: "/"
        /// </summary>
        public string VirtualHost { get; set; } = "/";
    }

    /// <summary>
    /// Connection Uri.
    /// </summary>
    public class ConnectUri
    {
        /// <summary>
        /// Amqp connect Uri
        /// </summary>
        public string ConnectionString { get; set; } = "amqp://guest:guest@localhost:5672/";
    }

    /// <summary>
    /// Producer Configurations
    /// </summary>
    public class ProducerConfig
    {
        public ProducerConfig([Required] string producerName)
        {
            ProducerName = producerName;
        }

        public string ProducerName { get; set; }
        public string? ExchangeName { get; set; }
        public string? RoutingKey { get; set; }
        public string Type { get; set; } = ExchangeType.Direct;
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;
        public IDictionary<string, object>? Arguments { get; set; } = null;
    }

    /// <summary>
    /// Consumer Configurations
    /// </summary>
    public class ConsumerConfig
    {
        public ConsumerConfig([Required] string consumerName)
        {
            ConsumerName = consumerName;
        }

        public string ConsumerName { get; set; }
        public string? QueueName { get; set; } = null;
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
        public IDictionary<string, object>? Arguments { get; set; } = null;
    }
}
