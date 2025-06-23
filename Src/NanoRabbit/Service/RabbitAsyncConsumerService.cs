using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NanoRabbit.Service
{
    public class RabbitAsyncConsumerService<TConfiguration> : BackgroundService
        where TConfiguration : RabbitConfiguration
    {
        private readonly ILogger<RabbitAsyncConsumerService<TConfiguration>> _logger;
        private readonly TConfiguration _configuration;
        private readonly ConsumerOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IChannel? _channel;
        private AsyncEventingBasicConsumer? _consumer;
        private string _consumerTag = string.Empty;
        private readonly string _instanceId; // Used to distinguish between different consumer instances

        public RabbitAsyncConsumerService(
            ILogger<RabbitAsyncConsumerService<TConfiguration>> logger,
            TConfiguration configuration,
            string consumerName,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (_configuration.Consumers == null) throw new ArgumentNullException(nameof(_configuration.Consumers));
            _options = _configuration.Consumers.FirstOrDefault(x => x.ConsumerName == consumerName) ??
                       throw new ArgumentNullException($"Could not find consumer options: {consumerName}");

            _serviceProvider = serviceProvider;
            _instanceId = $"{_options.ConsumerName}-{Guid.NewGuid().ToString("N")[..6]}"; // create a short instance id
            _logger.LogInformation("RabbitMQ Consumer Service [{InstanceId}] Initializing...", _instanceId);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service [{InstanceId}] Starting, Subscribing queue: {QueueName}",
                _instanceId,
                _options.QueueName);
            stoppingToken.Register(() =>
                _logger.LogInformation("RabbitMQ Consumer Service [{InstanceId}] Stopping...", _instanceId));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        await Connect(stoppingToken); // Reconnect
                    }

                    // Keep ExecuteAsync running, the actual work is done by the EventingBasicConsumer's event handler.
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // BackgroundService canceled, exit normally
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "RabbitMQ Consumer Service [{InstanceId}] An unhandled exception occurred. Will retry after 5 seconds...",
                        _instanceId);
                    // Close old resources that may exist
                    await CloseConnection();
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("RabbitMQ Consumer Service [{InstanceId}] Stopped.", _instanceId);
            await CloseConnection();
        }

        private async Task Connect(CancellationToken stoppingToken)
        {
            if (_connection != null && _connection.IsOpen) return; // Check if connected

            await CloseConnection(); // Close old resources that may exist

            var factory = new ConnectionFactory
            {
                HostName = _configuration.HostName,
                Port = _configuration.Port,
                UserName = _configuration.UserName,
                Password = _configuration.Password,
                VirtualHost = _configuration.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            try
            {
                _logger.LogInformation("RabbitMQ Consumer [{InstanceId}] Connecting to {HostName}:{Port}...",
                    _instanceId,
                    factory.HostName, factory.Port);
                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                _logger.LogInformation("RabbitMQ Consumer [{InstanceId}] Connected, Channel created.", _instanceId);

                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false,
                    cancellationToken: stoppingToken);
                _logger.LogInformation("RabbitMQ Consumer [{InstanceId}] QoS Set PrefetchCount={PrefetchCount}",
                    _instanceId,
                    _options.PrefetchCount);

                if (_options.DeclareQueue)
                {
                    _logger.LogInformation("RabbitMQ Consumer [{InstanceId}] Declaring Queue '{QueueName}'...",
                        _instanceId,
                        _options.QueueName);
                    await _channel.QueueDeclareAsync(queue: _options.QueueName,
                        durable: _options.QueueDurable,
                        exclusive: _options.QueueExclusive,
                        autoDelete: _options.QueueAutoDelete,
                        arguments: _options.QueueArguments,
                        cancellationToken: stoppingToken);
                }

                _consumer = new AsyncEventingBasicConsumer(_channel);
                _consumer.ReceivedAsync += async (_, ea) => { await HandleMessageReceived(ea, stoppingToken); };

                _consumerTag = await _channel.BasicConsumeAsync(queue: _options.QueueName, autoAck: _options.AutoAck,
                    consumer: _consumer, cancellationToken: stoppingToken);
                _logger.LogInformation(
                    "RabbitMQ Consumer [{InstanceId}] Subscribing to '{QueueName}'，ConsumerTag: {ConsumerTag}",
                    _instanceId, _options.QueueName, _consumerTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ Consumer [{InstanceId}] Connection or Setup Failure.", _instanceId);
                await CloseConnection();
                throw; // Throw an exception upwards for ExecuteAsync's retry logic to handle
            }
        }

        private async Task HandleMessageReceived(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
        {
            var messageBody = ea.Body.ToArray();
            var deliveryTag = ea.DeliveryTag;
            var correlationId = ea.BasicProperties.CorrelationId;

            _logger.LogDebug(
                "RabbitMQ Consumer [{InstanceId}] Received DeliveryTag={DeliveryTag}, CorrelationId='{CorrelationId}'",
                _instanceId, deliveryTag, correlationId);

            // Create a new Dependency Injection Scope for each message process
            // This is essential for working with Scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                var messageHandler =
                    scope.ServiceProvider.GetRequiredKeyedService<IAsyncMessageHandler>(_options.HandlerName);

                if (messageHandler == null)
                {
                    _logger.LogError(
                        "RabbitMQ Consumer [{InstanceId}] Unable to resolve IMessageHandler service. The message will not be processed.",
                        _instanceId);

                    try
                    {
                        _channel?.BasicNackAsync(deliveryTag, false, true, stoppingToken);
                    }
                    catch (Exception nackEx)
                    {
                        _logger.LogError(nackEx, "Nack failed.");
                    }

                    return;
                }

                try
                {
                    // Handle message
                    await messageHandler.HandleMessageAsync(messageBody, ea.RoutingKey, correlationId);

                    // Acknowledge or reject the message based on the result (if not AutoAck)
                    if (!_options.AutoAck)
                    {
                        try
                        {
                            _channel?.BasicAckAsync(deliveryTag, multiple: false, stoppingToken);
                            _logger.LogDebug(
                                "RabbitMQ Consumer [{InstanceId}] Ack Succeeded. DeliveryTag={DeliveryTag}",
                                _instanceId, deliveryTag);
                        }
                        catch (Exception ackNackEx)
                        {
                            _logger.LogError(ackNackEx,
                                "RabbitMQ Consumer [{InstanceId}] Ack failed. DeliveryTag={DeliveryTag}", _instanceId,
                                deliveryTag);
                            // TODO: Consider sending failed messages to a dead message queue or logging to a database
                        }
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        _channel?.BasicNackAsync(deliveryTag, multiple: false, requeue: false, stoppingToken);
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning("RabbitMQ Consumer [{InstanceId}] Nack Failed. DeliveryTag={DeliveryTag}",
                            _instanceId, deliveryTag);
                    }

                    _logger.LogError(ex,
                        "RabbitMQ Consumer [{InstanceId}] An exception occurred when calling IMessageHandler. DeliveryTag={DeliveryTag}, CorrelationId='{CorrelationId}'",
                        _instanceId, deliveryTag, correlationId);
                }
            } // Scope Dispose
        }

        private async Task CloseConnection()
        {
            if (_channel != null && _channel.IsOpen)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_consumerTag))
                    {
                        await _channel.BasicCancelAsync(_consumerTag); // Stop consuming
                    }

                    await _channel.CloseAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error [{InstanceId}] occurred while closing RabbitMQ channel.",
                        _instanceId);
                }

                _channel = null;
            }

            if (_connection != null && _connection.IsOpen)
            {
                try
                {
                    await _connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error [{InstanceId}] occurred while closing RabbitMQ connection.",
                        _instanceId);
                }

                _connection = null;
            }

            _logger.LogInformation("RabbitMQ connection and channel closed [{InstanceId}].", _instanceId);
        }

        public override void Dispose()
        {
            _logger.LogInformation("RabbitMQ Consumer Service [{InstanceId}] Disposing...", _instanceId);
            CloseConnection().GetAwaiter().GetResult();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}