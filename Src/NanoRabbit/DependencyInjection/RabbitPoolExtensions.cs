using Microsoft.Extensions.DependencyInjection;
using NanoRabbit.Connection;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitPoolExtensions
    {
        public static IServiceCollection AddRabbitPool(this IServiceCollection services, Action<List<ConnectOptions>> setupAction)
        {
            var options = new List<ConnectOptions>();
            setupAction(options);

            var pool = new RabbitPool();

            foreach (var option in  options)
            {
                pool.RegisterConnection(option.ConnectionName, option);
            }

            services.AddSingleton<IRabbitPool>(pool);

            return services;
        }
    }
}
