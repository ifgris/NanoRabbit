using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NanoRabbit.Consumer;

/// <summary>
/// RabbitMQ Synchronous Subscriber abstract class.
/// </summary>
public abstract class RabbitSubscriber : IHostedService
{
    private readonly ILogger<RabbitSubscriber>? _logger;
    private readonly IRabbitConsumer _consumer;
    private readonly string _consumerName;

    private readonly AutoResetEvent _exitSignal;
    private readonly List<Thread> _consumerThreads;


    protected RabbitSubscriber(IRabbitConsumer consumer, string consumerName, ILogger<RabbitSubscriber>? logger = null,
        int consumerCount = 1)
    {
        _consumer = consumer;
        _logger = logger;
        _consumerName = consumerName;
        _exitSignal = new AutoResetEvent(false);
        _consumerThreads = Enumerable.Range(0, consumerCount)
            .Select(_ => new Thread(() => Register(_exitSignal)))
            .ToList();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var thread in _consumerThreads)
        {
            thread.Start();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _exitSignal.Set();
        foreach (var thread in _consumerThreads)
        {
            thread.Join();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle messages method.
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

/// <summary>
/// RabbitMQ Asynchronous Subscriber abstract class.
/// </summary>
public abstract class RabbitAsyncSubscriber : IHostedService
{
    private readonly ILogger<RabbitAsyncSubscriber>? _logger;
    private readonly IRabbitConsumer _consumer;
    private readonly string _consumerName;
    private readonly int _consumerCount;
    private List<Task>? _consumerTasks;
    private CancellationTokenSource? _cancellationTokenSource;

    protected RabbitAsyncSubscriber(IRabbitConsumer consumer, string consumerName,
        ILogger<RabbitAsyncSubscriber>? logger, int consumerCount = 1)
    {
        _consumer = consumer;
        _logger = logger;
        _consumerCount = consumerCount;
        _consumerName = consumerName;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        _consumerTasks = Enumerable.Range(0, _consumerCount)
            .Select(_ => Task.Run(() => RegisterAsync(_cancellationTokenSource.Token)))
            .ToList();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // _cancellationTokenSource?.Cancel();
        // await Task.WhenAny(Task.Delay(Timeout.Infinite, cancellationToken));
        if (_consumerTasks != null)
        {
            _cancellationTokenSource?.Cancel();

            await Task.WhenAll(_consumerTasks);
        }
    }

    /// <summary>
    /// Handle messages task.
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