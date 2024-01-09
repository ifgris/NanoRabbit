using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NanoRabbit.Consumer;

public class RabbitSubscriber : IHostedService
{
    private readonly ILogger<RabbitSubscriber>? _logger;
    private readonly IRabbitConsumer _consumer;
    private readonly string _consumerName;
    private readonly ManualResetEventSlim _exitSignal;
    private readonly Thread _consumerThread;


    public RabbitSubscriber(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger = null)
    {
        _consumer = consumer;
        _logger = logger;
        _consumerName = consumerName;
        _exitSignal = new ManualResetEventSlim();
        _consumerThread = new Thread(() => Register(_exitSignal));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _consumerThread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _exitSignal.Set();
        _consumerThread.Join();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle messages
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual bool HandleMessage(string message)
    {
        Console.WriteLine(message);
        return true;
    }
    
    /// <summary>
    /// Register a consumer
    /// </summary>
    /// <param name="exitSignal"></param>
    private void Register(
        ManualResetEventSlim exitSignal
    )
    {
        var consumerOptions = _consumer.GetMe(_consumerName);

        var factory = new ConnectionFactory
        {
            HostName = consumerOptions.HostName,
            Port = consumerOptions.Port,
            UserName = consumerOptions.UserName,
            Password = consumerOptions.Password,
            VirtualHost = consumerOptions.VirtualHost,
            AutomaticRecoveryEnabled = consumerOptions.AutomaticRecoveryEnabled
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.BasicQos(consumerOptions.PrefetchSize, consumerOptions.PrefetchCount, false);
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                // handle incoming message
                _logger?.LogDebug($"Received message: {message}");
                var result = HandleMessage(message);
                if (result)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
            }
        };

        channel.BasicConsume(
            queue: consumerOptions.QueueName,
            autoAck: false,
            consumer: consumer);
        
        exitSignal.Wait();
    }
}

public class RabbitSubscriberAsync : IHostedService
{
    private readonly ILogger<RabbitSubscriberAsync>? _logger;
    private readonly IRabbitConsumer _consumer;
    private readonly string _consumerName;
    private CancellationTokenSource? _cancellationTokenSource;


    public RabbitSubscriberAsync(IRabbitConsumer consumer, string consumerName,
        ILogger<RabbitSubscriberAsync>? logger)
    {
        _consumer = consumer;
        _logger = logger;
        _consumerName = consumerName;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        await Task.Run(() => RegisterAsync(_cancellationTokenSource.Token));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        await Task.WhenAny(Task.Delay(Timeout.Infinite, cancellationToken));
    }

    /// <summary>
    /// Handle messages
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual Task HandleMessage(string message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Register a consumer
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task RegisterAsync(
        CancellationToken cancellationToken
    )
    {
        var consumerOptions = _consumer.GetMe(_consumerName);

        var factory = new ConnectionFactory
        {
            HostName = consumerOptions.HostName,
            Port = consumerOptions.Port,
            UserName = consumerOptions.UserName,
            Password = consumerOptions.Password,
            VirtualHost = consumerOptions.VirtualHost,
            AutomaticRecoveryEnabled = consumerOptions.AutomaticRecoveryEnabled
        };
        
        factory.DispatchConsumersAsync = true;

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.BasicQos(consumerOptions.PrefetchSize, consumerOptions.PrefetchCount, false);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                // handle incoming message
                _logger?.LogDebug($"Received message: {message}");
                var result = HandleMessage(message);
                if (result.IsCompleted)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                await Task.Yield();
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
            }
        };

        channel.BasicConsume(
            queue: consumerOptions.QueueName,
            autoAck: false,
            consumer: consumer);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
        }
    }
}