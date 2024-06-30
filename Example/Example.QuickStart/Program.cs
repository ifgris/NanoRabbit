using NanoRabbit.Connection;
using NanoRabbit.Helper;

var rabbitHelper = new RabbitHelper(rabbitConfig: new RabbitConfiguration
{
    HostName = "localhost",
    Port = 5672,
    VirtualHost = "/",
    UserName = "admin",
    Password = "admin",
    Producers = new List<ProducerOptions> { new ProducerOptions {
        ProducerName = "FooProducer",
        ExchangeName = "amq.topic",
        RoutingKey = "foo.key"
    } }
});

rabbitHelper.Publish<string>("FooProducer", "Hello from NanoRabbit");
