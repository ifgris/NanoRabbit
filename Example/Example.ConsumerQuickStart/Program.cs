using Microsoft.Extensions.Logging;
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

public class ConsumeService : RabbitSubscriber
{
    public ConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriber>? logger, string consumerName) : base(consumer, consumerName, logger)
    {
    }

    protected override bool HandleMessage(string message)
    {
        Console.WriteLine(message);
        return true;
    }
}