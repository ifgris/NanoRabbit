namespace NanoRabbit.NanoRabbit
{
    /// <summary>
    /// Connection options.
    /// </summary>
    public class ConnectOptions
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
}
