![NanoRabbit logo](https://raw.githubusercontent.com/cgcel/NanoRabbit/master/Img/logo.png)

[![.NET](https://github.com/cgcel/NanoRabbit/actions/workflows/dotnet.yml/badge.svg?branch=master&event=push)](https://github.com/cgcel/NanoRabbit/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/NanoRabbit.svg)](https://nuget.org/packages/NanoRabbit)

## About

NanoRabbit, A Lightweight RabbitMQ .NET API for .NET 6.

## Installation

You can get NanoRabbit by grabbing the latest [NuGet](https://www.nuget.org/packages/NanoRabbit) package. 

## Version

| NanoRabbit | RabbitMQ.Client |
| :---: | :---: |
| 0.0.1 | 6.5.0 |
| 0.0.2 | 6.5.0 |
| 0.0.3 | 6.5.0 |

## QuickStart

Note: NanoRabbit **heavily relies** on Naming Connections, Producers, Consumers.

Follow the codes below to start using NanoRabbit!

For more, please visit the [Examples](https://github.com/cgcel/NanoRabbit/tree/master/Example).

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
    c.Add(new ConnectOptions("Connection1")
    {
        ConnectConfig = new ConnectConfig
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin",
            VirtualHost = "DATA"
        },
        ProducerConfigs = new List<ProducerConfig> { 
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

    c.Add(new ConnectOptions("Connection2")
    {
        // ...
    });
});
```

Then, you can use IRabbitPool at anywhere, for example, create a publisher backgroungservice:

```csharp
public class PublishService : BackgroundService
{
    private readonly IRabbitPool _rabbitPool;
    private readonly ILogger<PublishService> _logger;

    public PublishService(IRabbitPool rabbitPool, ILogger<PublishService> logger)
    {
        _rabbitPool = rabbitPool;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _rabbitPool.SimplePublish("Connection1", "DataBasicQueueProducer", "Hello from conn1");
            _logger.LogInformation("Conn 1");
            _rabbitPool.SimplePublish("Connection2", "HostBasicQueueProducer", "Hello from conn2");
            _logger.LogInformation("Conn 2");
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

## Contributing

1. Fork this repository.
2. Create a new branch in you current repos from the **dev** branch.
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
