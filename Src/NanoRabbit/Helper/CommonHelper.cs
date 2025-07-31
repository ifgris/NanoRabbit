using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace NanoRabbit.Helper
{
    public static class CommonHelper
    {
        /// <summary>
        /// Read NanoRabbit configs in appsettings.json
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static TRabbitConfiguration? ReadSettings<TRabbitConfiguration>(this IConfiguration configuration)
        {
            var configSection = configuration.GetSection(typeof(TRabbitConfiguration).Name);
            if (!configSection.Exists())
            {
                throw new Exception($"Configuration section '{typeof(TRabbitConfiguration).Name}' not found.");
            }

            var rabbitConfig = configSection.Get<TRabbitConfiguration>();
            return rabbitConfig;
        }

        public static ConnectionFactory GetConnectionFactory<TRabbitConfiguration>(TRabbitConfiguration configuration)
        where TRabbitConfiguration : RabbitConfiguration
        {
            
            if (!string.IsNullOrEmpty(configuration.Uri))
            {
                var uri = new Uri(configuration.Uri);
                return new ConnectionFactory
                {
                    SocketFactory = null,
                    AmqpUriSslProtocols = SslProtocols.None,
                    AuthMechanisms = null,
                    AutomaticRecoveryEnabled = true,
                    ConsumerDispatchConcurrency = 0,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                    HandshakeContinuationTimeout = default,
                    ContinuationTimeout = default,
                    EndpointResolverFactory = null,
                    RequestedConnectionTimeout = default,
                    SocketReadTimeout = default,
                    SocketWriteTimeout = default,
                    Ssl = null,
                    TopologyRecoveryEnabled = false,
                    TopologyRecoveryFilter = null,
                    TopologyRecoveryExceptionHandler = null,
                    Endpoint = null,
                    ClientProperties = null,
                    CredentialsProvider = null,
                    RequestedChannelMax = 0,
                    RequestedFrameMax = 0,
                    RequestedHeartbeat = default,
                    MaxInboundMessageBodySize = 0,
                    Uri = uri,
                    ClientProvidedName = null
                };
            }
            else
            {
                return new ConnectionFactory
                {
                    SocketFactory = null,
                    AmqpUriSslProtocols = SslProtocols.None,
                    AuthMechanisms = null,
                    AutomaticRecoveryEnabled = true,
                    ConsumerDispatchConcurrency = 0,
                    HostName = configuration.HostName,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                    HandshakeContinuationTimeout = default,
                    ContinuationTimeout = default,
                    EndpointResolverFactory = null,
                    Port = configuration.Port,
                    RequestedConnectionTimeout = default,
                    SocketReadTimeout = default,
                    SocketWriteTimeout = default,
                    Ssl = null,
                    TopologyRecoveryEnabled = false,
                    TopologyRecoveryFilter = null,
                    TopologyRecoveryExceptionHandler = null,
                    Endpoint = null,
                    ClientProperties = null,
                    UserName = configuration.UserName,
                    Password = configuration.Password,
                    CredentialsProvider = null,
                    RequestedChannelMax = 0,
                    RequestedFrameMax = 0,
                    RequestedHeartbeat = default,
                    VirtualHost = configuration.VirtualHost,
                    MaxInboundMessageBodySize = 0,
                    ClientProvidedName = null
                };
            }
        }
    }
}