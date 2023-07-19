using NanoRabbit.Connection;
using RabbitMQ.Client;

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
    ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("DataBasicQueueProducer")
        {
            ExchangeName = "BASIC.TOPIC",
            RoutingKey = "BASIC.KEY",
            Type = ExchangeType.Topic
        }
    }
});
pool.RegisterConnection(new ConnectOptions("Connection2")
{
    ConnectUri = new()
    {
        ConnectionString = "amqp://admin:admin@localhost:5672/HOST"
    },
    ProducerConfigs = new List<ProducerConfig> 
    {
        new ProducerConfig("HostBasicQueueProducer")
        {
            ExchangeName = "BASIC.TOPIC",
            RoutingKey = "BASIC.KEY",
            Type = ExchangeType.Topic
        }
    }
});

while (true)
{
    pool.SimplePublish("Connection1", "DataBasicQueueProducer", "Hello from conn1");
    await Task.Delay(1000);

    pool.SimplePublish("Connection2", "HostBasicQueueProducer", "Hello from conn2");
    await Task.Delay(1000);
}