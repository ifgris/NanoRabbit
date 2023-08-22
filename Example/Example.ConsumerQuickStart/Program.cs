using Example.ConsumerQuickStart;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;
using NanoRabbit.Logging;

var pool = new RabbitPool(config => { config.EnableLogging = true; });

pool.RegisterConnection(new ConnectOptions("Connection1", option =>
{
    option.ConnectConfig = new(config =>
    {
        config.HostName = "localhost";
        config.Port = 5672;
        config.UserName = "admin";
        config.Password = "admin";
        config.VirtualHost = "FooHost";
    });
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("FooFirstQueueProducer", c =>
        {
            c.ExchangeName = "FooTopic";
            c.RoutingKey = "FooFirstKey";
            c.Type = ExchangeType.Topic;
        })
    };
    option.ConsumerConfigs = new List<ConsumerConfig>
    {
        new ConsumerConfig("FooFirstQueueConsumer", c => { c.QueueName = "FooFirstQueue"; })
    };
}));

var logger = GlobalLogger.CreateLogger<RabbitConsumer<string>>();

var basicConsumer = new BasicConsumer("Connection1", "FooFirstQueueConsumer", pool, logger);
// var queueNameConsumer = new QueueNameConsumer("FooFirstQueue", pool, logger);

basicConsumer.StartConsuming();
// queueNameConsumer.StartConsuming();