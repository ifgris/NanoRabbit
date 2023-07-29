using NanoRabbit.Connection;
using NanoRabbit.Logging;

var logger = GlobalLogger.CreateLogger<RabbitPool>();
var pool = new RabbitPool(logger);
pool.RegisterConnection(new ConnectOptions("Connection1", option =>
{
    option.ConnectConfig = new(config =>
    {
        config.HostName = "localhost";
        config.Port = 5672;
        config.UserName = "admin";
        config.Password = "admin";
        config.VirtualHost = "DATA";
    });
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("DataBasicQueueProducer", c =>
        {
            c.ExchangeName = "BASIC.TOPIC";
            c.RoutingKey = "BASIC.KEY";
            c.Type = ExchangeType.Topic;
        })
    };
    option.ConsumerConfigs = new List<ConsumerConfig>
    {
        new ConsumerConfig("DataBasicQueueConsumer", c =>
        {
            c.QueueName = "BASIC_QUEUE";
        })
    };
}));

Thread publishThread = new Thread(() =>
{
    while (true)
    {
        pool.SimplePublish<string>("Connection1", "DataBasicQueueProducer", "Hello from SimplePublish<T>()!");
        Console.WriteLine("Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});

Thread consumeThread = new Thread(() =>
{
    while (true)
    {
        pool.SimpleReceive<string>("Connection1", "DataBasicQueueConsumer", handler =>
        {
            Console.WriteLine($"Received: {handler}");
        });
        Thread.Sleep(1000);
    }
});

publishThread.Start();
consumeThread.Start();
