using Microsoft.Extensions.Logging.Abstractions;
using NanoRabbit;

namespace Test.Logger
{
    [TestClass]
    public class NullLoggerTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var logger = NullLogger.Instance;

            var rabbitHelper = await RabbitHelper.CreateAsync(rabbitConfig: new RabbitConfiguration
            {
                HostName = "localhost",
                Port = 5672,
                VirtualHost = "/",
                UserName = "admin",
                Password = "admin",
                Producers = new List<ProducerOptions> { 
                    new ProducerOptions {
                        ProducerName = "FooProducer",
                        ExchangeName = "amq.topic",
                        RoutingKey = "foo.key"
                    }
                },
                Consumers = new List<ConsumerOptions> { 
                    new ConsumerOptions {
                        ConsumerName= "FooConsumer",
                        QueueName = "foo-queue"
                    }
                }
            }, logger);

            await rabbitHelper.PublishAsync<string>("FooProducer", "Hello from NanoRabbit");
        }
    }
}