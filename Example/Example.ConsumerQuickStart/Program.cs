using NanoRabbit.Connection;
using NanoRabbit.Consumer;

var consumer = new RabbitConsumer(new[]
{
    new ConsumerOptions
    {
        ConsumerName = "FooSecondQueueConsumer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        QueueName = "FooSecondQueue"
    }
});

while (true)
{
    consumer.Receive("FooSecondQueueConsumer", message =>
    {
        Console.WriteLine(message);
    });
}