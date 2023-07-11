using Microsoft.Extensions.DependencyInjection;

namespace NanoRabbit.NanoRabbit
{
    public static class RabbitPoolExtensions
    {
        public static IServiceCollection AddRabbitMQWrapper(this IServiceCollection services, IDictionary<string, ConnectOptions> connectionOptions)
        {
            var wrapper = new RabbitPool();

            foreach (var entry in connectionOptions)
            {
                wrapper.RegisterConnection(entry.Key, entry.Value);
            }

            services.AddSingleton(wrapper);

            return services;
        }
    }
}
