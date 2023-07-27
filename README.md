![NanoRabbit logo](https://raw.githubusercontent.com/cgcel/NanoRabbit/master/Img/logo.png)

[![build](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml/badge.svg?branch=master&event=push)](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml) [![NuGet](https://img.shields.io/nuget/v/NanoRabbit.svg)](https://nuget.org/packages/NanoRabbit) [![Nuget Downloads](https://img.shields.io/nuget/dt/NanoRabbit)](https://www.nuget.org/packages/NanoRabbit) [![License](https://img.shields.io/github/license/cgcel/NanoRabbit)](https://github.com/cgcel/NanoRabbit)

## About

NanoRabbit, A Lightweight RabbitMQ .NET API for .NET 6.

## Installation

You can get NanoRabbit by grabbing the latest [NuGet](https://www.nuget.org/packages/NanoRabbit) package. 

## Version

| NanoRabbit | RabbitMQ.Client |
| :---: | :---: |
| 0.0.1, 0.0.2, 0.0.3, 0.0.4 | 6.5.0 |

## Document

The NanoRabbit Document is at [NanoRabbit Wiki](https://github.com/cgcel/NanoRabbit/wiki).

## QuickStart

> NanoRabbit is designed as a library depends on **NAMING** Connections, Producers, Consumers. So it's important to set a **UNIQUE NAME** for each Connections, Producers, Consumers.

For more, please visit the [Examples](https://github.com/cgcel/NanoRabbit/tree/master/Example).

### Register a Connection

Register a RabbitMQ Connection by instantiating `RabbitPool`, and configure the producer and consumer.

```csharp
var pool = new RabbitPool();
pool.RegisterConnection(new ConnectOptions("Connection1", option =>
{
    option.ConnectConfig = new(config =>
    {
        config.HostName = "localhost";
        config.Port = 5672;
        config.UserName = "admin";
        config.Password = "admin";
        config.VirtualHost = "DATA";
    });
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("DataBasicQueueProducer", c =>
        {
            c.ExchangeName = "BASIC.TOPIC";
            c.RoutingKey = "BASIC.KEY";
            c.Type = ExchangeType.Topic;
        })
    };
    option.ConsumerConfigs = new List<ConsumerConfig>
    {
        new ConsumerConfig("DataBasicQueueConsumer", c =>
        {
            c.QueueName = "BASIC_QUEUE";
        })
    };
}));
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

There is also a easy-to-use `RabbitProducer`, which used to publish messages without `ConnectionName` and `ProducerConfig`, for more, read [Wiki](https://github.com/cgcel/NanoRabbit/wiki/Producer).

### Receive messages

Instantiate a consumer in Program.cs:

```csharp
var consumer = new BasicConsumer("Connection1", "DataBasicQueueConsumer", pool);
await Task.Run(async () =>
{
    while (true)
    {
        Console.WriteLine("Start receiving...");
        consumer.Receive();
        await Task.Delay(1000);
    }
});
```

Create your own consumer, for example: BasicConsumer.cs:

```csharp
public class BasicConsumer : RabbitConsumer<string>
{
    public BasicConsumer(string connectionName, string consumerName, IRabbitPool pool) : base(connectionName, consumerName, pool)
    {
    }

    protected override void MessageHandler(string message)
    {
        Console.WriteLine($"Receive: {message}");
    }
}
```

### DependencyInjection

Register IRabbitPool in Program.cs:

```csharp
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitPool(c =>
{
    c.Add(new ConnectOptions("Connection1", option =>
    {
        option.ConnectConfig = new(config =>
        {
            config.HostName = "localhost";
            config.Port = 5672;
            config.UserName = "admin";
            config.Password = "admin";
            config.VirtualHost = "DATA";
        });
        option.ProducerConfigs = new List<ProducerConfig>
        {
            new ProducerConfig("DataBasicQueueProducer", c =>
            {
                c.ExchangeName = "BASIC.TOPIC";
                c.RoutingKey = "BASIC.KEY";
                c.Type = ExchangeType.Topic;
            })
        };
        option.ConsumerConfigs = new List<ConsumerConfig>
        {
            new ConsumerConfig("DataBasicQueueConsumer", c =>
            {
                c.QueueName = "BASIC_QUEUE";
            })
        };
    }));

    c.Add(new ConnectOptions("Connection2", option =>
    {
        // ...
    }));
});
```

Then, you can use IRabbitPool at anywhere.

More DI Usage at [Wiki](https://github.com/cgcel/NanoRabbit/wiki/DependencyInjection).

## Contributing

1. Fork this repository.
2. Create a new branch in you current repos from the **dev** branch.
3. Push commits and create a Pull Request (PR) to NanoRabbit.

## TODO

- [x] Basic Consume & Publish support
- [x] DependencyInjection support
- [ ] Logging support
- [ ] ASP.NET support
- [ ] Exchange Configurations

## Thanks

- Visual Studio 2022
- [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Masstransit](https://github.com/masstransit/masstransit)
- [EasyNetQ](https://github.com/autofac/Autofac)

## LICENSE

NanoRabbit is licensed under the Apache-2.0 license.
