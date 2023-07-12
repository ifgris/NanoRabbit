using NanoRabbit.NanoRabbit;
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
    }
});

while (true)
{
    pool.Publish<string>("Connection1", "DataBasicQueueProducer", "Hello from Publish<T>()!");

    Console.WriteLine("Sent to RabbitMQ");

    await Task.Delay(1000);
}

//while (true)
//{
//    pool.Receive("Connection1", "BASIC_QUEUE", body =>
//    {
//        Console.WriteLine(Encoding.UTF8.GetString(body));
//    });
//    Task.Delay(1000);
//}