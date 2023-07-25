using Example.QuickStart;
using NanoRabbit.Connection;
using NanoRabbit.Producer;

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
    option.ConsumerConfigs = new List<ConsumerConfig>
    {
        new ConsumerConfig("DataBasicQueueConsumer", c =>
        {
            c.QueueName = "BASIC_QUEUE";
        })
    };
})
);

await Task.Run(async () =>
{
    while (true)
    {
        pool.SimplePublish<string>("Connection1", "DataBasicQueueProducer", "Hello from SimplePublish<T>()!");
        Console.WriteLine("Sent to RabbitMQ");
        await Task.Delay(1000);
    }
});

var producer = new RabbitProducer("Connection1", "DataBasicQueueProducer", pool);
await Task.Run(async () =>
{
    while (true)
    {
        producer.Publish<string>("Hello from Publish<T>()!");
        Console.WriteLine("Sent to RabbitMQ");
        await Task.Delay(1000);
    }
});

var consumer = new BasicConsumer("Connection1", "DataBasicQueueConsumer", pool);
await Task.Run(async () =>
{
    while (true)
    {
        Console.WriteLine("Start receiving...");
        consumer.Receive();
        await Task.Delay(1000);
    }
});
