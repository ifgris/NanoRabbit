using NanoRabbit.DependencyInjection;
using Example.ReadSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
    
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitProducerFromAppSettings(builder.Configuration);
builder.Services.AddRabbitConsumerFromAppSettings(builder.Configuration);

builder.Services.AddHostedService<PublishService>();
builder.Services.AddRabbitSubscriber<ConsumeService>("FooFirstQueueConsumer", enableLogging: true);

var host = builder.Build();
await host.RunAsync();
