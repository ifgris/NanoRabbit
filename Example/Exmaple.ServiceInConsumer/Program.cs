using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<IRedisConnectionFactory>(provider =>
{
    var connStr = provider.GetRequiredService<IConfiguration>().GetSection("DbConfig").GetSection(nameof(RedisConfig)).Get<RedisConfig>().DbConnStr;
    return new RedisConnectionFactory(connStr);
});

builder.Services.AddKeyedRabbitHelper("test", builder =>
{
    builder.SetHostName("localhost")
        .SetPort(5672)
        .SetVirtualHost("test")
        .SetUserName("admin")
        .SetPassword("admin")
        .AddProducerOption(producer =>
        {
            producer.ProducerName = "FooProducer";
            producer.ExchangeName = "amq.topic";
            producer.RoutingKey = "foo.key";
            producer.Type = ExchangeType.Topic;
        });
});

builder.Services.AddKeyedRabbitHelper("default", builder =>
{
    builder.SetHostName("localhost")
        .SetPort(5672)
        .SetVirtualHost("/")
        .SetUserName("admin")
        .SetPassword("admin")
        .UseAsyncConsumer(true) // set UseAsyncConsumer to true
        .AddConsumerOption(consumer =>
        {
            consumer.ConsumerName = "FooConsumer";
            consumer.QueueName = "foo-queue";
        });
})
.AddKeyedAsyncRabbitConsumer<FooQueueHandler>("default", "FooConsumer", consumers: 1);

// Test redis service
//builder.Services.AddHostedService<TestHostedService>();

var host = builder.Build();
await host.RunAsync();

public class FooQueueHandler : DefaultAsyncMessageHandler
{
    private readonly IRedisConnectionFactory _connFactory;
    private readonly IRabbitHelper _rabbitHelper;

    public FooQueueHandler(IServiceProvider serviceProvider,
        IRedisConnectionFactory connFactory
        )
    {
        _rabbitHelper = serviceProvider.GetRabbitHelper("test");
        _connFactory = connFactory;
    }

    public override async Task HandleMessageAsync(string message)
    {
        Console.WriteLine($"[x] Received from foo-queue: {message}");

        var redisConn = _connFactory.GetConnection();
        var redisDb = redisConn.GetDatabase();
        await redisDb.StringSetAsync("1", message);

        _rabbitHelper.Publish("FooProducer", message);

    }
}

public class TestHostedService : BackgroundService
{
    private readonly IRedisConnectionFactory _connFactory;
    private readonly IRabbitHelper _rabbitHelper;

    public TestHostedService(IRedisConnectionFactory connFactory, IServiceProvider serviceProvider)
    {
        _connFactory = connFactory;
        _rabbitHelper = serviceProvider.GetRabbitHelper("test");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            var redisConn = _connFactory.GetConnection();
            var redisDb = redisConn.GetDatabase();

            var nowTime = DateTime.Now.ToLongTimeString();
            await redisDb.StringSetAsync("1", nowTime);

            _rabbitHelper.Publish("FooProducer", nowTime);
            await Task.Delay(1000, stoppingToken);
        }
    }
}