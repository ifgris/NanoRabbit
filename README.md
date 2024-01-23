![NanoRabbit logo](https://raw.githubusercontent.com/cgcel/NanoRabbit/master/Img/logo.png)

[![NuGet](https://img.shields.io/nuget/v/NanoRabbit.svg)](https://nuget.org/packages/NanoRabbit) [![Nuget Downloads](https://img.shields.io/nuget/dt/NanoRabbit)](https://www.nuget.org/packages/NanoRabbit) [![License](https://img.shields.io/github/license/cgcel/NanoRabbit)](https://github.com/cgcel/NanoRabbit)
[![codebeat badge](https://codebeat.co/badges/a37a04d9-dd8e-4177-9b4c-c17526910f7e)](https://codebeat.co/projects/github-com-cgcel-nanorabbit-master)

## About

NanoRabbit, A **Lightweight** RabbitMQ .NET 3rd party library for .NET 6 and up, which makes a simple way to manage **Multiple** connections, producers, consumers, and easy to use.

> _NanoRabbit is under development! Please note that some APIs may change their names or usage!_

## Building

| Branch |                                                                                 Building Status                                                                                 |
|:------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
| master | [![build](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml/badge.svg?branch=master&event=push)](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml) |
|  dev   |  [![build](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml/badge.svg?branch=dev&event=push)](https://github.com/cgcel/NanoRabbit/actions/workflows/build.yml)   |

## Features

- Customize the name of producers, consumers.
- Dependency injection available.
- Multiple connections, producers, and consumers can be created.

## Installation

You can get NanoRabbit by grabbing the latest [NuGet](https://www.nuget.org/packages/NanoRabbit) package.

See [Wiki](https://github.com/cgcel/NanoRabbit/wiki/Installation) for more details.

## Version

|                   NanoRabbit                    | RabbitMQ.Client |     .NET      |
|:-----------------------------------------------:|:---------------:|:-------------:|
|    0.0.1, 0.0.2, 0.0.3, 0.0.4, 0.0.5, 0.0.6     |      6.5.0      |      6.0      |
|                      0.0.7                      |      6.5.0      | 6.0, 7.0, 8.0 |
|                  0.0.8, 0.0.9                   |      6.7.0      | 6.0, 7.0, 8.0 |
| 0.1.0, 0.1.1, 0.1.2, 0.1.3, 0.1.4, 0.1.5, 0.1.6 |      6.8.1      | 6.0, 7.0, 8.0 |

## Document

The NanoRabbit Document is at [NanoRabbit Wiki](https://github.com/cgcel/NanoRabbit/wiki).

## QuickStart

> _NanoRabbit is designed as a library depends on **NAMING** Producers, Consumers. So it's important to set
a **UNIQUE NAME** for each Producers, Consumers._

For more, please visit the [Examples](https://github.com/cgcel/NanoRabbit/tree/master/Example).

### Create a RabbitProducer && RabbitConsumer

#### RabbitProducer

Register a RabbitMQ Producer by calling `RabbitProducer()`, and configure it.

```csharp
var producer = new RabbitProducer(new[]
{
    new ProducerOptions
    {
        ProducerName = "FooFirstQueueProducer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        ExchangeName = "amq.topic",
        RoutingKey = "FooFirstKey",
        Type = ExchangeType.Topic,
        Durable = true,
        AutoDelete = false,
        AutomaticRecoveryEnabled = true
    }
});
```

#### RabbitConsumer

Register a RabbitMQ Consumer by calling `RabbitConsumer()`, and configure it.

```csharp
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
```

### Simple Publish

[After](#rabbitproducer) creating the `RabbitProducer`, you can simply publish a message by calling `Publish<T>()`.

```csharp
producer.Publish<string>("FooFirstQueueProducer", "Hello");
```

### Simple Consume

[After](#rabbitconsumer) creating the `RabbitConsumer`, you can simply consume a message by
inheriting `RabbitSubscriber`.

```csharp
public class ConsumeService : RabbitSubscriber
{
    public ConsumeService(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger = null) : base(consumer, consumerName, logger)
    {
        // ...
    }

    protected override bool HandleMessage(string message)
    {
        // ...
        return true;
    }
}

var consumeService = new ConsumeService(consumer, null, "FooSecondQueueConsumer");

consumeService.StartAsync(CancellationToken.None);
```

### Forward messages

> Working on it.

### DependencyInjection

#### AddRabbitProducer

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitProducer(options =>
{
    options.AddProducer(new ProducerOptions
    {
        ProducerName = "FooFirstQueueProducer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        ExchangeName = "amq.topic",
        RoutingKey = "FooFirstKey",
        Type = ExchangeType.Topic,
        Durable = true,
        AutoDelete = false,
        Arguments = null,
        AutomaticRecoveryEnabled = true
    });
    options.AddProducer(new ProducerOptions
    {
        ProducerName = "BarFirstQueueProducer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "BarHost",
        ExchangeName = "amq.direct",
        RoutingKey = "BarFirstKey",
        Type = ExchangeType.Direct,
        Durable = true,
        AutoDelete = false,
        Arguments = null,
        AutomaticRecoveryEnabled = true
    });
});
```

#### AddRabbitConsumer

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitConsumer(options =>
{
    options.AddConsumer(new ConsumerOptions
    {
        ConsumerName = "FooFirstQueueConsumer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "FooHost",
        QueueName = "FooFirstQueue",
        AutomaticRecoveryEnabled = true
    });
    options.AddConsumer(new ConsumerOptions
    {
        ConsumerName = "BarFirstQueueConsumer",
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "BarHost",
        QueueName = "BarFirstQueue", AutomaticRecoveryEnabled = true
    });
});
```

#### AddRabbitSubscriber

After adding `RabbitConsumer` and inheriting `RabbitSubscriber`, you should register the BackgroundService
by `AddRabbitSubscriber`:

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitSubscriber<ConsumeService>("FooSecondQueueConsumer");
```

From **0.1.6** on, setting `Consumer Count` in AddRabbitSubscriber() is available.

#### Using producer or consumer

Then, you can use RabbitProducer and RabbitConsumer at anywhere.

For example:

```csharp
public class ConsumeService : RabbitSubscriber
{
    private readonly IRabbitProducer _producer;

    public ConsumeService(IRabbitConsumer consumer, ILogger<RabbitSubscriber>? logger, string consumerName, IRabbitProducer producer) : base(consumer, consumerName, logger)
    {
        _producer = producer;
    }

    protected override bool HandleMessage(string message)
    {
        _producer.Publish("FooSecondQueueProducer", message);
        return true;
    }
}
```

More DI Usage at [Wiki](https://github.com/cgcel/NanoRabbit/wiki/DependencyInjection).

## Contributing

1. Fork this repository.
2. Create a new branch in you current repos from the **dev** branch.
3. Push commits and create a Pull Request (PR) to NanoRabbit.

## Todo

- [x] Basic Consume & Publish support
- [x] DependencyInjection support
- [x] Logging support
- [ ] Forward messages
- [ ] ASP.NET support
- [ ] Exchange Configurations
- [x] .NET 7 support
- [x] .NET 8 support
- [ ] Caching of failed sends

## Thanks

- Visual Studio 2022
- [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Masstransit](https://github.com/masstransit/masstransit)
- [EasyNetQ](https://github.com/autofac/Autofac)

## License

NanoRabbit is licensed under the [MIT](https://github.com/cgcel/NanoRabbit/blob/dev/LICENSE.txt) license.
