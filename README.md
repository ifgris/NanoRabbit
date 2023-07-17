# NanoRabbit

NanoRabbit, a Lightweight RabbitMQ .NET API.

## Installation

You can get NanoRabbit by grabbing the latest NuGet package. 

## Version

| NanoRabbit | RabbitMQ.Client |
| :---: | :---: |
| 0.0.1 | 6.5.0 |

## QuickStart

Follow the examples below to start using NanoRabbit!

### Simple Publish

```csharp
var pool = new RabbitPool();
pool.RegisterConnection("Connection1", new ConnectOptions
{
    ConnectConfig = new()
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin",
        VirtualHost = "DATA"
    }
});

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

## Contributing

1. Fork this repository.
2. Create a new branch in you current repos from the 'dev' branch.
3. Push commits and create a Pull Request (PR) to NanoRabbit.

## TODO

- [ ] DependencyInjection support
- [ ] Logging support
- [ ] ASP.NET support

## Thanks

- Visual Studio 2022
- [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Masstransit](https://github.com/masstransit/masstransit)
- [EasyNetQ](https://github.com/autofac/Autofac)

## LICENSE

NanoRabbit is licensed under the Apache-2.0 license
