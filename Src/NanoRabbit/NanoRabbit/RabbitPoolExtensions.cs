using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Tracing;

namespace NanoRabbit.NanoRabbit
{
    public static class RabbitPoolExtensions
    {
        public static IServiceCollection AddRabbitPool(this IServiceCollection services, IDictionary<string, ConnectOptions> connectionOptions)
        {
            var pool = new RabbitPool();

            foreach (var entry in connectionOptions)
            {
                pool.RegisterConnection(entry.Key, entry.Value);
            }

            services.AddSingleton(pool);

            return services;
        }
    }
}
