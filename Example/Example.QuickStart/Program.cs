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

Task publishTask = Task.Run(() =>
{
    while (true)
    {
        pool.NanoPublish<string>("Connection1", "FooFirstQueueProducer", "Hello from SimplePublish<T>()!");
        Console.WriteLine("Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});
Task consumeTask = Task.Run(() =>
{
    while (true)
    {
        pool.NanoConsume<string>("Connection1", "FooFirstQueueConsumer",
            msg => { Console.WriteLine($"Received: {msg}"); });
        Thread.Sleep(1000);
    }
});

Task.WaitAll(publishTask, consumeTask);
