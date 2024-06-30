using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NanoRabbit.Connection;

public class RabbitConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private readonly RabbitConfiguration _rabbitConfiguration;

    public RabbitConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
        _rabbitConfiguration = new RabbitConfiguration
        {
            Producers = new List<ProducerOptions>(),
            Consumers = new List<ConsumerOptions>()
        };
    }

    public void SetHostName(string hostName)
    {
        _rabbitConfiguration.HostName = hostName;
    }
    public void SetPort(int port)
    {
        _rabbitConfiguration.Port = port;
    }
    public void SetVirtualHost(string virtualHost)
    {
        _rabbitConfiguration.VirtualHost = virtualHost;
    }
    public void SetUserName(string userName)
    {
        _rabbitConfiguration.UserName = userName;
    }
    public void SetPassword(string password)
    {
        _rabbitConfiguration.Password = password;
    }
    
    public void UseAsyncConsumer(bool useAsyncConsumer)
    {
        _rabbitConfiguration.UseAsyncConsumer = useAsyncConsumer;
    }

    public void AddProducer(ProducerOptions options)
    {
        _rabbitConfiguration.Producers.Add(options);
    }
    
    public void AddConsumer(ConsumerOptions options)
    {
        _rabbitConfiguration.Consumers.Add(options);
    }

    public RabbitConfiguration Build()
    {
        return _rabbitConfiguration;
    }
}

public static class ConfigurationExtensions
{
    /// <summary>
    /// Read NanoRabbit configs in appsettings.json
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static RabbitConfiguration? ReadSettings(this IConfiguration configuration)
    {
        var rabbitConfig = configuration.GetSection("NanoRabbit").Get<RabbitConfiguration>();
        return rabbitConfig;
    }
}