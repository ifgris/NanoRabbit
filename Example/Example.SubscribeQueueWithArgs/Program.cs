using Example.QuickStart;
using NanoRabbit.Connection;

var pool = new RabbitPool();
pool.RegisterConnection(new ConnectOptions("Connection1")
{
    ConnectConfig = new()
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "DATA"
    },
    ConsumerConfigs = new List<ConsumerConfig>
    {
        new ConsumerConfig("DataBasicQueueConsumer")
        {
            QueueName = "BASIC_QUEUE",
            Arguments = new Dictionary<string, object>
            {
                {"x-message-ttl", 259200000 }
            }
        }
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
