![NanoRabbit logo](https://raw.githubusercontent.com/cgcel/NanoRabbit/master/Img/logo.png)

[![NuGet](https://img.shields.io/nuget/v/NanoRabbit.svg)](https://nuget.org/packages/NanoRabbit) [![Nuget Downloads](https://img.shields.io/nuget/dt/NanoRabbit)](https://www.nuget.org/packages/NanoRabbit) [![License](https://img.shields.io/github/license/cgcel/NanoRabbit)](https://github.com/cgcel/NanoRabbit) 
[![codebeat badge](https://codebeat.co/badges/a37a04d9-dd8e-4177-9b4c-c17526910f7e)](https://codebeat.co/projects/github-com-cgcel-nanorabbit-master)

## About

NanoRabbit, A **Lightweight** RabbitMQ .NET API for .NET 6 and up, which makes a simple way to manage **Multiple** connections, producers, consumers, and easy to use.

> _NanoRabbit is under development! Please note that some APIs may change their names or usage!_

## Building

| Branch |                                                                                 Building Status                                                                                 |
|:------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
| master | [![build](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml/badge.svg?branch=master&event=push)](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml) | 
|  dev   |  [![build](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml/badge.svg?branch=dev&event=push)](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml)   |  

## Features

- Customize the name of connections, producers, consumers.
- Dependency injection available.
- Multiple connections, producers, and consumers can be created.

## Installation

You can get NanoRabbit by grabbing the latest [NuGet](https://www.nuget.org/packages/NanoRabbit) package. 

See [Wiki](https://github.com/cgcel/NanoRabbit/wiki/Installation) for more details.

## Version

|                NanoRabbit                | RabbitMQ.Client | .NET |
|:----------------------------------------:|:---:|:---:|
| 0.0.1, 0.0.2, 0.0.3, 0.0.4, 0.0.5, 0.0.6 | 6.5.0 | 6.0 |
|                  0.0.7                   | 6.5.0 | 6.0, 7.0, 8.0 |

## Document

The NanoRabbit Document is at [NanoRabbit Wiki](https://github.com/cgcel/NanoRabbit/wiki).

## QuickStart

> _NanoRabbit is designed as a library depends on **NAMING** Connections, Producers, Consumers. So it's important to set a **UNIQUE NAME** for each Connections, Producers, Consumers._

For more, please visit the [Examples](https://github.com/cgcel/NanoRabbit/tree/master/Example).

### Register a Connection

Register a RabbitMQ Connection by instantiating `RabbitPool`, and configure the producer and consumer.

```csharp
var pool = new RabbitPool(config => { config.EnableLogging = true; });
pool.RegisterConnection(new ConnectOptions("Connection1", option =>
{
    option.ConnectConfig = new(config =>
    {
        config.HostName = "localhost";
        config.Port = 5672;
        config.UserName = "admin";
        config.Password = "admin";
        config.VirtualHost = "FooHost";
    });
    option.ProducerConfigs = new List<ProducerConfig>
    {
        new ProducerConfig("FooFirstQueueProducer", c =>
        {
            c.ExchangeName = "FooTopic";
            c.RoutingKey = "FooFirstKey";
            c.Type = ExchangeType.Topic;
        })
    };
    option.ConsumerConfigs = new List<ConsumerConfig>
    {
        new ConsumerConfig("FooFirstQueueConsumer", c => { c.QueueName = "FooFirstQueue"; })
    };
}));
```

### Simple Publish

After registering the `RabbitPool`, you can simply publish a message by calling `NanoPublish<T>()`.

```csharp
Task publishTask = Task.Run(() =>
{
    while (true)
    {
        pool.NanoPublish<string>("Connection1", "FooFirstQueueProducer", "Hello from SimplePublish<T>()!");
        Console.WriteLine("Sent to RabbitMQ");
        Thread.Sleep(1000);
    }
});
Task.WaitAll(publishTask);
```

There is also a easy-to-use `RabbitProducer`, which used to publish messages without `ConnectionName` and `ProducerConfig`, for more, read [Wiki](https://github.com/cgcel/NanoRabbit/wiki/Producer).

### Simple Consume

After registering the `RabbitPool`, you can simply consume a message by calling `NanoConsume<T>()`.

```csharp
Task consumeTask = Task.Run(() =>
{
    while (true)
    {
        pool.NanoConsume<string>("Connection1", "FooFirstQueueConsumer",
            msg => { Console.WriteLine($"Received: {msg}"); });
        Thread.Sleep(1000);
    }
});
Task.WaitAll(consumeTask);
```

There is also a easy-to-use `RabbitConsumer`, which used to consume messages without `ConnectionName` and `ProducerConfig`, for more, read [Wiki](https://github.com/cgcel/NanoRabbit/wiki/Consumer).

### Forward messages

Sometimes we have to consume messages from Foo RabbitMQ and publish the same message to Bar RabbitMQ, NanoRabbit provides a simple method to forward message, using the method called `NanoForward<T>()`.

```csharp
Task forwardTask = Task.Run(() =>
{
     while (true)
     {
         pool.NanoForward<string>("Connection1", "FooFirstQueueConsumer", "Connection2", "FooQueueProducer");
         Thread.Sleep(1000);
     }
});
Task.WaitAll(forwardTask);
```

### DependencyInjection

Register IRabbitPool in Program.cs:

```csharp
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddRabbitPool(
    globalConfig => { globalConfig.EnableLogging = true; },
    c =>
    {
        c.Add(new ConnectOptions("Connection1", option =>
        {
            option.ConnectConfig = new(config =>
            {
                config.HostName = "localhost";
                config.Port = 5672;
                config.UserName = "admin";
                config.Password = "admin";
                config.VirtualHost = "FooHost";
            });
            option.ProducerConfigs = new List<ProducerConfig>
            {
                new ProducerConfig("FooFirstQueueProducer", c =>
                {
                    c.ExchangeName = "FooTopic";
                    c.RoutingKey = "FooFirstKey";
                    c.Type = ExchangeType.Topic;
                })
            };
            option.ConsumerConfigs = new List<ConsumerConfig>
            {
                new ConsumerConfig("FooFirstQueueConsumer", c => { c.QueueName = "FooFirstQueue"; })
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

## Todo

- [x] Basic Consume & Publish support
- [x] DependencyInjection support
- [x] Logging support
- [x] Forward messages
- [x] Using Task in Consumers and Producers
- [ ] ASP.NET support
- [ ] Exchange Configurations
- [x] .NET 7 support
- [x] .NET 8 support
- [x] RabbitMQ reconnecting

## Thanks

- Visual Studio 2022
- [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Masstransit](https://github.com/masstransit/masstransit)
- [EasyNetQ](https://github.com/autofac/Autofac)

## License

NanoRabbit is licensed under the MIT license.
