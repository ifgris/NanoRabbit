![NanoRabbit logo](https://raw.githubusercontent.com/cgcel/NanoRabbit/master/Img/logo.png)

[![NuGet](https://img.shields.io/nuget/v/NanoRabbit.svg)](https://nuget.org/packages/NanoRabbit) [![Nuget Downloads](https://img.shields.io/nuget/dt/NanoRabbit)](https://www.nuget.org/packages/NanoRabbit) [![License](https://img.shields.io/github/license/cgcel/NanoRabbit)](https://github.com/cgcel/NanoRabbit)

## About

NanoRabbit, A **Lightweight** RabbitMQ .NET 3rd party library for .NET 8 and up, which makes a simple way to manage
**Multiple** *connections*, *producers*, *consumers*, and easy to use.

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

|  NanoRabbit   | RabbitMQ.Client |     .NET      |
|:-------------:|:---------------:|:-------------:|
| 0.0.1 ~ 0.1.8 |    obsolete     |   obsolete    |
| 0.1.9 ~ 0.2.3 |   6.5.0-6.8.1   | 6.0, 7.0, 8.0 |
|     0.2.4     |   6.5.0-6.8.1   |      8.0      |
| 0.3.0, 0.3.1  |      7.1.2      |      8.0      |

## Document

For details, see: [NanoRabbit Wiki](https://github.com/cgcel/NanoRabbit/wiki).

## QuickStart

> *NanoRabbit is designed as a library depends on **NAMING** Connections, Producers and Consumers. So it's important to
set
a **UNIQUE NAME** for each Connection, Producer and Consumer.*

> From RabbitMQ.Client 7.0.0 (NanoRabbit 0.3.0) on, synchronously functions are not suppoerted.

For more, please visit the [Examples](https://github.com/cgcel/NanoRabbit/tree/master/Example).

### Setup Connections && Helpers && RabbitConsumers

#### Setup connections

Before using NanoRabbit, you should register `IConnection` and other configs by calling `AddRabbitConnection(
this IServiceCollection services,
Action<RabbitConfigurationBuilder> builder)`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure the RabbitMQ Connection
builder.Services.AddRabbitConnection(x =>
    {
        x.SetHostName("localhost")
            .SetPort(5672)
            .SetVirtualHost("/")
            .SetUserName("admin")
            .SetPassword("admin")
            .SetConnectionName("FooConnection")
            .AddProducerOption(producer =>
            {
                producer.ProducerName = "FooProducer";
                producer.ExchangeName = "amq.topic";
                producer.RoutingKey = "foo.key";
                producer.Type = ExchangeType.Topic;
            })
            .AddConsumerOption(consumer =>
            {
                consumer.ConsumerName = "FooConsumer";
                consumer.QueueName = "foo-queue";
                consumer.ConsumerCount = 3;
                consumer.HandlerName = nameof(FooQueueHandler);
            })
            .AddConsumerOption(consumer =>
            {
                consumer.ConsumerName = "BarConsumer";
                consumer.QueueName = "bar-queue";
                consumer.ConsumerCount = 2;
                consumer.HandlerName = nameof(BarQueueHandler);
            });
    });

// todo: Add RabbitHelpers and RabbitCnsumers if neccessary.

using IHost host = builder.Build();

host.Run();

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
```

#### Add Consumers

Register a RabbitMQ Consumer by calling `AddRabbitAsyncHandler<TAsyncHandler>()` and `AddRabbitConsumer()`, you should
declare `HandlerName` at the previous settings of RabbitConnection.

```csharp
// Register the connection like before
builder.Services.AddRabbitConnection(x=>{});

// Register IAsyncHandler and RabbitConsumer
builder.Services.AddRabbitAsyncHandler<FooQueueHandler>()
    .AddRabbitAsyncHandler<BarQueueHandler>()
    .AddRabbitConsumer();

// Build and run the host
using IHost host = builder.Build();
host.Run();
```

When the host starts running, it will start some `BackgroundService` to start consuming.

### Publish Messages

[After](#setup-connections) registering `RabbitConnection`, you should register `RabbitHelper`, then you can simply
publish a message by
calling `PublishAsync<T>(string producerName, T message)`.

```csharp
// Register the connection like before
builder.Services.AddRabbitConnection(x=>{});

// Register a RabbitHelper
builder.Services.AddRabbitHelper();

// Build and run the host
using IHost host = builder.Build();
host.Run();

var rabbitMqHelper = host.Services.GetRequiredService<IRabbitHelper>();
await rabbitMqHelper.PublishAsync("FooProducer", "Hello, World!");
```

### Consume Messages

[After](#add-consumers) registering a `RabbitConsumer`, you should complete the TAsyncHandler, which is declared in
`AddRabbitConnection(x=>{})` and being registered by calling `AddRabbitAsyncHandler<TAsyncHandler>()`.

For example:

```csharp
public class FooQueueHandler : IAsyncMessageHandler
{
    public async Task HandleMessageAsync(byte[] messageBody, string? routingKey = null, string? correlationId = null)
    {
        var message = Encoding.UTF8.GetString(messageBody);
        Console.WriteLine($"[x] Received from foo-queue: {message}");
        await Task.Delay(1000);
        Console.WriteLine("[x] Done");
    }
}
```

Inherit IAsyncMessageHandler and implement
`Task HandleMessageAsync(byte[] messageBody, string? routingKey = null, string? correlationId = null)`.

### Forward messages

> Working on it.

More Usage at [Wiki](https://github.com/cgcel/NanoRabbit/wiki).

## Contributing

1. Fork this repository.
2. Create a new branch in you current repos from the **dev** branch.
3. Push commits and create a Pull Request (PR) to NanoRabbit.

## Todo

- [x] Basic Consume & Publish
- [x] DependencyInjection
- [x] Logging
- [x] Support .NET latest frameworks
- [ ] Forward messages
- [x] ASP.NET support
- [x] TLS support
- [x] Re-trying of failed sends
- [x] Basic [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client) functions extensions

## Thanks

- Visual Studio 2022
- [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Masstransit](https://github.com/masstransit/masstransit)
- [EasyNetQ](https://github.com/autofac/Autofac)
- [Polly.Core](https://github.com/App-vNext/Polly)

## License

NanoRabbit is licensed under the [MIT](https://github.com/cgcel/NanoRabbit/blob/dev/LICENSE.txt) license.
