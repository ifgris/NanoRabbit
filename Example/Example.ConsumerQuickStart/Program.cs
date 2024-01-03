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
        QueueName = "FooSecondQueue",
        AutomaticRecoveryEnabled = true
    }
});

int count = 0;
while (true)
{
    consumer.Receive("FooSecondQueueConsumer", message =>
    {
        count++;
        Console.WriteLine(count);
        Console.WriteLine(message);
    });
    Task.Delay(1000);
}