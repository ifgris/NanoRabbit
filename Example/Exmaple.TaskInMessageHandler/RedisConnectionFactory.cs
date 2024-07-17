using StackExchange.Redis;

public interface IRedisConnectionFactory
{
    ConnectionMultiplexer GetConnection();
    Task<ConnectionMultiplexer> GetConnectionAsync();
}

public class RedisConnectionFactory : IRedisConnectionFactory
{
    private readonly string _redisConnectionString;

    public RedisConnectionFactory(string redisConnectionString)
    {
        _redisConnectionString = redisConnectionString;
    }

    public ConnectionMultiplexer GetConnection()
    {
        return ConnectionMultiplexer.Connect(_redisConnectionString);
    }
    
    public Task<ConnectionMultiplexer> GetConnectionAsync()
    {
        return ConnectionMultiplexer.ConnectAsync(_redisConnectionString);
    }
}