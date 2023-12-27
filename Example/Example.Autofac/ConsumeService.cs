﻿using Microsoft.Extensions.Hosting;
using NanoRabbit.Consumer;

namespace Example.Autofac;

public class ConsumeService : BackgroundService
{
    private readonly IRabbitConsumer _consumer;

    public ConsumeService(IRabbitConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _consumer.Receive("FooFirstQueueConsumer", HandleMessage,
                prefetchCount: 500);
        }

        await Task.Delay(10 * 1000, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }

    private void HandleMessage(string message)
    {
        Console.WriteLine(message);
    }
}