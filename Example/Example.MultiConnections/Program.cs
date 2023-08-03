using NanoRabbit.Connection;

var pool = new RabbitPool(config => { config.EnableLogging = true; });

pool.RegisterConnection(new ConnectOptions("Connection1", option =>
{
    option.ConnectConfig = new ConnectConfig(config =>
    {
        config.HostName = "localhost";
        config.Port = 5672;
        config.UserName = "admin";
        config.Password = "admin";
        config.VirtualHost = "FooHost";
    });
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new("FooFirstQueueProducer", c =>
        {
            c.ExchangeName = "FooTopic";
            c.RoutingKey = "FooFirstKey";
            c.Type = ExchangeType.Topic;
        }),
        new("FooSecondQueueProducer", c =>
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
        new("BarFirstQueueProducer", c =>
        {
            c.ExchangeName = "BarDirect";
            c.RoutingKey = "BarFirstKey";
            c.Type = ExchangeType.Direct;
        })
    };
}));

var fooFirstThread = new Thread(() =>
{
    while (true)
    {
        pool.NanoPublish("Connection1", "FooFirstQueueProducer", "Hello from conn1");
        Thread.Sleep(1000);
    }
});

var fooSecondThread = new Thread(() =>
{
    while (true)
    {
        pool.NanoPublish("Connection1", "FooSecondQueueProducer", "Hello from conn1");
        Thread.Sleep(1000);
    }
});

var barFirstThread = new Thread(() =>
{
    while (true)
    {
        pool.NanoPublish("Connection2", "BarFirstQueueProducer", "Hello from conn2");
        Thread.Sleep(1000);
    }
});

fooFirstThread.Start();
fooSecondThread.Start();
barFirstThread.Start();