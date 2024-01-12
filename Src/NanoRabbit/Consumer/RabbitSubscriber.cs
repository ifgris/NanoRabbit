using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NanoRabbit.Consumer;

public abstract class RabbitSubscriber : IHostedService
{
    private readonly ILogger<RabbitSubscriber>? _logger;
    private readonly IRabbitConsumer _consumer;
    private readonly string _consumerName;
    private readonly AutoResetEvent _exitSignal;
    private readonly Thread _consumerThread;


    protected RabbitSubscriber(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger = null)
    {
        _consumer = consumer;
        _logger = logger;
        _consumerName = consumerName;
        _exitSignal = new AutoResetEvent(false);
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
    protected abstract bool HandleMessage(string message);
    
    /// <summary>
    /// Register a consumer
    /// </summary>
    /// <param name="exitSignal"></param>
    private void Register(AutoResetEvent exitSignal)
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
        
        exitSignal.WaitOne(); // wait for signal
    }
}

public abstract class RabbitAsyncSubscriber : IHostedService
{
    private readonly ILogger<RabbitAsyncSubscriber>? _logger;
    private readonly IRabbitConsumer _consumer;
    private readonly string _consumerName;
    private CancellationTokenSource? _cancellationTokenSource;

    protected RabbitAsyncSubscriber(IRabbitConsumer consumer, string consumerName,
        ILogger<RabbitAsyncSubscriber>? logger)
    {
        _consumer = consumer;
        _logger = logger;
        _consumerName = consumerName;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => RegisterAsync(_cancellationTokenSource.Token));
        return Task.CompletedTask;
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
    protected abstract Task HandleMessageAsync(string message);

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
                // support Task and async Task
                await HandleMessageAsync(message);
                channel.BasicAck(ea.DeliveryTag, false);
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