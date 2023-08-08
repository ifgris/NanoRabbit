using NanoRabbit.Connection;

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

pool.RegisterConnection(new ConnectOptions("Connection2", option =>
{
    option.ConnectConfig = new(config =>
    {
        config.HostName = "localhost";
        config.Port = 5673;
        config.UserName = "guest";
        config.Password = "guest";
        config.VirtualHost = "/";
    });
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("FooQueueProducer", c =>
        {
            c.ExchangeName = "FooTopic";
            c.RoutingKey = "FooKey";
            c.Type = ExchangeType.Topic;
        })
    };
}));

Task publishTask = Task.Run(() =>
{
     while (true)
     {
         pool.NanoPublish<string>("Connection1", "FooFirstQueueProducer", "Hello from SimplePublish<T>()!");
         Console.WriteLine("Sent to RabbitMQ");
         Thread.Sleep(1000);
     }
});
Task forwardTask = Task.Run(() =>
{
     while (true)
     {
         pool.NanoForward<string>("Connection1", "FooFirstQueueConsumer", "Connection2", "FooQueueProducer");
         Thread.Sleep(1000);
     }
});

Task.WaitAll(publishTask, forwardTask);
