using Microsoft.Extensions.Logging;
using NanoRabbit;

namespace Test.Management
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestExchangeDeclare()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            var rabbitHelper = await RabbitHelper.CreateAsync(rabbitConfig: new RabbitConfiguration
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin",
                Port = 5672,
                VirtualHost = "/",
                Producers = new List<ProducerOptions>
                {
                    new ProducerOptions
                    {
                        ProducerName = "FooProducer",
                        ExchangeName = "amq.topic",
                        RoutingKey = "foo.key",
                        Type = ExchangeType.Topic
                    }
                }
            }, logger);

            var channel = await rabbitHelper.GetChannelAsync("FooProducer");
            await rabbitHelper.ExchangeDeclareAsync(channel, "test.topic", ExchangeType.Topic);
        }
        
        [TestMethod]
        public async Task TestQueueDeclare()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            var rabbitHelper = await RabbitHelper.CreateAsync(rabbitConfig: new RabbitConfiguration
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin",
                Port = 5672,
                VirtualHost = "/",
                Producers = new List<ProducerOptions>
                {
                    new ProducerOptions
                    {
                        ProducerName = "FooProducer",
                        ExchangeName = "amq.topic",
                        RoutingKey = "foo.key",
                        Type = ExchangeType.Topic
                    }
                }
            }, logger);

            var channel = await rabbitHelper.GetChannelAsync("FooProducer");
            await rabbitHelper.QueueDeclareAsync(channel, "test-queue");
        }
        
        [TestMethod]
        public async Task TestQueueBind()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            var rabbitHelper = await RabbitHelper.CreateAsync(rabbitConfig: new RabbitConfiguration
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin",
                Port = 5672,
                VirtualHost = "/",
                Producers = new List<ProducerOptions>
                {
                    new ProducerOptions
                    {
                        ProducerName = "FooProducer",
                        ExchangeName = "amq.topic",
                        RoutingKey = "foo.key",
                        Type = ExchangeType.Topic
                    }
                }
            }, logger);

            var channel = await rabbitHelper.GetChannelAsync("FooProducer");
            await rabbitHelper.QueueBindAsync(channel, "test-queue", "test.topic", "test.key");
        }
        
        [TestMethod]
        public async Task TestQueueDelete()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            var rabbitHelper = await RabbitHelper.CreateAsync(rabbitConfig: new RabbitConfiguration
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin",
                Port = 5672,
                VirtualHost = "/",
                Producers = new List<ProducerOptions>
                {
                    new ProducerOptions
                    {
                        ProducerName = "FooProducer",
                        ExchangeName = "amq.topic",
                        RoutingKey = "foo.key",
                        Type = ExchangeType.Topic
                    }
                }
            }, logger);

            var channel = await rabbitHelper.GetChannelAsync("FooProducer");
            await rabbitHelper.QueueDeleteAsync(channel, "test-queue", false, false);
        }
        
        [TestMethod]
        public async Task TestQueuePurge()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            var rabbitHelper = await RabbitHelper.CreateAsync(rabbitConfig: new RabbitConfiguration
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin",
                Port = 5672,
                VirtualHost = "/",
                Producers = new List<ProducerOptions>
                {
                    new ProducerOptions
                    {
                        ProducerName = "FooProducer",
                        ExchangeName = "amq.topic",
                        RoutingKey = "foo.key",
                        Type = ExchangeType.Topic
                    }
                }
            }, logger);

            var channel = await rabbitHelper.GetChannelAsync("FooProducer");
            await rabbitHelper.QueuePurgeAsync(channel, "test-queue");
        }
    }
}