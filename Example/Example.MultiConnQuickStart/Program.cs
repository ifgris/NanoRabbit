using NanoRabbit.Connection;

var pool = new RabbitPool(config =>
{
    config.EnableLogging = true;
});

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
        }),
        new ProducerConfig("FooSecondQueueProducer", c =>
        {
            c.ExchangeName = "FooTopic";
            c.RoutingKey = "FooSecondKey";
            c.Type = ExchangeType.Topic;
        })
    };
}));

pool.RegisterConnection(new ConnectOptions("Connection2", option =>
{
    option.ConnectUri = new ConnectUri("amqp://admin:admin@localhost:5672/BarHost");
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("BarFirstQueueProducer", c =>
        {
            c.ExchangeName = "BarDirect";
            c.RoutingKey = "BarFirstKey";
            c.Type = ExchangeType.Direct;
        })
    };
}));

Thread fooFirstThread = new Thread(() =>
{
    while (true)
    {
        pool.SimplePublish("Connection1", "FooFirstQueueProducer", "Hello from conn1");
        Console.WriteLine("FooFirstQueueProducer Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});

Thread fooSecondThread = new Thread(() =>
{
    while (true)
    {
        pool.SimplePublish("Connection1", "FooSecondQueueProducer", "Hello from conn1");
        Console.WriteLine("FooSecondQueueProducer Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});

Thread barFirstThread = new Thread(() =>
{
    while (true)
    {
        pool.SimplePublish("Connection2", "BarFirstQueueProducer", "Hello from conn2");
        Console.WriteLine("BarFirstQueueProducer Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});

fooFirstThread.Start();
fooSecondThread.Start();
barFirstThread.Start();