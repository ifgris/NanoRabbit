using NanoRabbit.Connection;
using NanoRabbit.Producer;

var producer = new RabbitProducer(new[]
{
    new ProducerOptions
    {
        ProducerName = "FooFirstQueueProducer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        ExchangeName = "amq.topic",
        RoutingKey = "FooFirstKey",
        Type = ExchangeType.Topic,
        Durable = true,
        AutoDelete = false,
        Arguments = null,
    },
    new ProducerOptions
    {
        ProducerName = "BarFirstQueueProducer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "BarHost",
        ExchangeName = "amq.direct",
        RoutingKey = "BarFirstKey",
        Type = ExchangeType.Direct,
        Durable = true,
        AutoDelete = false,
        Arguments = null,
    },
});

producer.Publish("FooFirstQueueProducer", "Hello");
producer.Publish("BarFirstQueueProducer", "Hello");