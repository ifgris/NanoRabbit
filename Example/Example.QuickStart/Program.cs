using Example.QuickStart;
using NanoRabbit.Connection;
using NanoRabbit.Producer;
using RabbitMQ.Client;

var pool = new RabbitPool();
pool.RegisterConnection("Connection1", new ConnectOptions
{
    ConnectConfig = new()
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "DATA"
    },
    ProducerConfigs = new Dictionary<string, ProducerConfig>
    {
        {
            "DataBasicQueueProducer", 
            new ProducerConfig
            {
                ExchangeName = "BASIC.TOPIC",
                RoutingKey = "BASIC.KEY",
                Type = ExchangeType.Topic
            }
        }
    },
    ConsumerConfigs = new Dictionary<string, ConsumerConfig>
    {
        {
            "DataBasicQueueConsumer",
            new ConsumerConfig
            {
                QueueName = "BASIC_QUEUE"
            }
        }
    }
});

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
await Task.Run(async() =>
{
    while (true)
    {
        Console.WriteLine("Start receiving...");
        consumer.Receive();
        await Task.Delay(1000);
    }
});
