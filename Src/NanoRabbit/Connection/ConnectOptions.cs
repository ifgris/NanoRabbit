using System.ComponentModel.DataAnnotations;

namespace NanoRabbit.Connection;

/// <summary>
/// GlobalConfig class
/// </summary>
public class GlobalConfig
{
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Connection options.
/// </summary>
public class ConnectOptions
{
    /// <summary>
    /// Create a new RabbitMQ Connection.
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="config"></param>
    public ConnectOptions([Required] string connectionName, Action<ConnectOptions> config)
    {
        ConnectionName = connectionName;
        config(this);
    }

    public string ConnectionName { get; set; }
    public ConnectConfig? ConnectConfig { get; set; }
    public ConnectUri? ConnectUri { get; set; }
    public List<ProducerConfig>? ProducerConfigs { get; set; }
    public List<ConsumerConfig>? ConsumerConfigs { get; set; }
}

/// <summary>
/// Connection configurations
/// </summary>
public class ConnectConfig
{
    /// <summary>
    /// Connection configuration.
    /// </summary>
    /// <param name="config"></param>
    public ConnectConfig(Action<ConnectConfig> config)
    {
        config(this);
    }

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
    public ConnectUri(string connectionString)
    {
        ConnectionString = connectionString;
    }

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
    /// <summary>
    /// Producer configuration.
    /// </summary>
    /// <param name="producerName"></param>
    public ProducerConfig([Required] string producerName, Action<ProducerConfig> config)
    {
        ProducerName = producerName;
        config(this);
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
    /// <summary>
    /// Consumer configuration.
    /// </summary>
    /// <param name="consumerName"></param>
    /// <param name="config"></param>
    public ConsumerConfig([Required] string consumerName, Action<ConsumerConfig> config)
    {
        ConsumerName = consumerName;
        config(this);
    }

    public string ConsumerName { get; set; }
    public string? QueueName { get; set; } = null;
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object>? Arguments { get; set; } = null;
}

/// <summary>
/// Convenience class providing compile-time names for standard exchange types.
/// </summary>
/// <remarks>
/// Use the static members of this class as values for the
/// "exchangeType" arguments for IModel methods such as
/// ExchangeDeclare. The broker may be extended with additional
/// exchange types that do not appear in this class.
/// </remarks>
public static class ExchangeType
{
    /// <summary>
    /// Exchange type used for AMQP direct exchanges.
    /// </summary>
    public const string Direct = "direct";

    /// <summary>
    /// Exchange type used for AMQP fanout exchanges.
    /// </summary>
    public const string Fanout = "fanout";

    /// <summary>
    /// Exchange type used for AMQP headers exchanges.
    /// </summary>
    public const string Headers = "headers";

    /// <summary>
    /// Exchange type used for AMQP topic exchanges.
    /// </summary>
    public const string Topic = "topic";

    private static readonly string[] s_all = { Fanout, Direct, Topic, Headers };

    /// <summary>
    /// Retrieve a collection containing all standard exchange types.
    /// </summary>
    public static ICollection<string> All()
    {
        return s_all;
    }
}