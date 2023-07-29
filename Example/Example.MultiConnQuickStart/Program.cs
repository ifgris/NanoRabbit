using NanoRabbit.Connection;

var pool = new RabbitPool();
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
}));

pool.RegisterConnection(new ConnectOptions("Connection2", option =>
{
    option.ConnectUri = new ConnectUri("amqp://admin:admin@localhost:5672/HOST");
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("HostBasicQueueProducer", c =>
        {
            c.ExchangeName = "BASIC.DIRECT";
            c.RoutingKey = "BASIC.KEY";
            c.Type = ExchangeType.Direct;
        })
    };
}));

Thread fooThread = new Thread(() =>
{
    while (true)
    {
        pool.SimplePublish("Connection1", "DataBasicQueueProducer", "Hello from conn1");
        Console.WriteLine("Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});

Thread barThread = new Thread(() =>
{
    while (true)
    {
        pool.SimplePublish("Connection2", "HostBasicQueueProducer", "Hello from conn2");
        Console.WriteLine("Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});

fooThread.Start();
barThread.Start();