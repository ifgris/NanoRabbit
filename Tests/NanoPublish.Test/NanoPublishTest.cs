using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NanoRabbit.Connection;

namespace NanoPublish.Test;

[TestClass]
public class NanoPublishTest
{
    [TestMethod]
    public void TestNanoPublish()
    {
        var pool = new RabbitPool(c => { c.EnableLogging = true; });
        
        // pool.RegisterConnection(new ConnectOptions("FooConnection", option =>
        // {
        //     option.ConnectConfig = new(config =>
        //     {
        //         config.HostName = "localhost";
        //         config.Port = 5672;
        //         config.UserName = "admin";
        //         config.Password = "admin";
        //         config.VirtualHost = "FooHost";
        //     });
        //     option.ProducerConfigs = new List<ProducerConfig>
        //     {
        //         new ProducerConfig("FooFirstQueueProducer", c =>
        //         {
        //             c.ExchangeName = "FooTopic";
        //             c.RoutingKey = "FooFirstKey";
        //             c.Type = ExchangeType.Topic;
        //         })
        //     };
        // }));
        
        pool.NanoPublish("FooConnection", "FooFirstQueueProducer", "NanoPublishTest");
    }
}