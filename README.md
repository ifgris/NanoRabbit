# NanoRabbit

NanoRabbit, a Lightweight RabbitMQ .NET API.

## Installation

You can get NanoRabbit by grabbing the latest [NuGet](https://www.nuget.org/packages/NanoRabbit) package. 

## Version

| NanoRabbit | RabbitMQ.Client |
| :---: | :---: |
| 0.0.1 | 6.5.0 |
| 0.0.2 | 6.5.0 |

## QuickStart

Note: NanoRabbit **heavily relies** on Naming Connections, Producers, Consumers.

Follow the examples below to start using NanoRabbit!

### Register a Connection

Register a RabbitMQ Connection by instantiating `RabbitPool`, and configure the producer and consumer.

```csharp
var pool = new RabbitPool();
pool.RegisterConnection(new ConnectOptions("Connection1")
{
    ConnectConfig = new()
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "DATA"
    },
    ProducerConfigs = new List<ProducerConfig> 
    {
        new ProducerConfig("DataBasicQueueProducer")
        {
            ExchangeName = "BASIC.TOPIC",
            RoutingKey = "BASIC.KEY",
            Type = ExchangeType.Topic
        }
    },
    ConsumerConfigs = new List<ConsumerConfig>
    {
        new ConsumerConfig("DataBasicQueueConsumer")
        {
            QueueName = "BASIC_QUEUE"
        }
    }
});
```

### Simple Publish

After registering the `RabbitPool`, you can simply publish a message by calling `SimplePublish<T>()`.

```csharp
await Task.Run(async () =>
{
    while (true)
    {
        pool.SimplePublish<string>("Connection1", "DataBasicQueueProducer", "Hello from SimplePublish<T>()!");
        Console.WriteLine("Sent to RabbitMQ");
        await Task.Delay(1000);
    }
});
```

### Receive messages

Working...

## Contributing

1. Fork this repository.
2. Create a new branch in you current repos from the 'dev' branch.
3. Push commits and create a Pull Request (PR) to NanoRabbit.

## TODO

- [x] DependencyInjection support
- [ ] Logging support
- [ ] ASP.NET support

## Thanks

- Visual Studio 2022
- [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Masstransit](https://github.com/masstransit/masstransit)
- [EasyNetQ](https://github.com/autofac/Autofac)

## LICENSE

NanoRabbit is licensed under the Apache-2.0 license.
