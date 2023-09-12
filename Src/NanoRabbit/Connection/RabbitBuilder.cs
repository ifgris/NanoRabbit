using RabbitMQ.Client;

namespace NanoRabbit.Connection;


public class RabbitBuilder
{
    private readonly IConnectionFactory _connectionFactory;

    public RabbitBuilder(Action<RabbitConfig> globalConfigAction, 
        Action<IRabbitConfigurator> configAction) 
    {
        var config = new RabbitConfig();
        globalConfigAction(config);

        _connectionFactory = new ConnectionFactory() 
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username, 
            Password = config.Password,
            VirtualHost = config.VirtualHost
        };

        var configurator = new RabbitConfigurator(_connectionFactory);
        configAction(configurator); 
    }

    public IConnection Build()
    {
        return _connectionFactory.CreateConnection();
    }
}

public class RabbitConfig 
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; } 
    public string Password { get; set; }
    public string VirtualHost { get; set; }
}

public interface IRabbitConfigurator
{
    void AddConnection(string name, Action<IConnectionOption> config);  
}

public class RabbitConfigurator : IRabbitConfigurator
{
    private readonly IConnectionFactory _factory;

    public RabbitConfigurator(IConnectionFactory factory)
    {
        _factory = factory; 
    }

    public void AddConnection(string name, 
        Action<IConnectionOption> config)
    {
        var option = new ConnectionOption(_factory);
        config(option);
    }
}