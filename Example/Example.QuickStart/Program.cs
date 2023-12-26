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
        AutomaticRecoveryEnabled = true
    }
});

producer.Publish("FooFirstQueueProducer", "Hello");
