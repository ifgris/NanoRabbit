using Example.ConsumerQuickStart;
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

var consumeService = new ConsumeService(consumer, null, "FooSecondQueueConsumer");

consumeService.StartAsync(CancellationToken.None);