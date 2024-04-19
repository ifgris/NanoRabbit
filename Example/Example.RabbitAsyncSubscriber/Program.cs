using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddRabbitConsumer(builder =>
{
   builder.AddConsumer(new ConsumerOptions
   {
      ConsumerName = "FooFirstQueueConsumer",
      HostName = "localhost",
      Port = 5672,
      UserName = "admin",
      Password = "admin",
      VirtualHost = "FooHost",
      QueueName = "FooFirstQueue",
      AutomaticRecoveryEnabled = true,
      PrefetchCount = 2
   }); 
});

builder.Services.AddRabbitAsyncSubscriber<ConsumerService>("FooFirstQueueConsumer");

var host = builder.Build();
await host.RunAsync();

public class ConsumerService : RabbitAsyncSubscriber
{
   private int _count;

    public ConsumerService(IRabbitConsumer consumer, string consumerName, ILogger<RabbitAsyncSubscriber>? logger, int consumerCount = 1) : base(consumer, consumerName, logger, consumerCount)
    {
        _count = 0;
    }

    // protected override Task HandleMessage(string message)
    // {
    //    Task.Run(async () =>
    //    {
    //       _count++;
    //       Console.WriteLine($"{_count}: {message}");
    //       await Task.Delay(1000); // make a delay
    //    });
    //    return Task.CompletedTask;
    // }

    protected override async Task HandleMessageAsync(string message)
   {
      await Task.Run(async () =>
      {
         _count++;
         Console.WriteLine($"{_count}: {message}");
         await Task.Delay(1000); // make a delay
      });
   }
}

