using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitPoolExtensions
    {
        public static IServiceCollection AddRabbitPool(this IServiceCollection services, Action<List<ConnectOptions>> setupAction)
        {
            var options = new List<ConnectOptions>();
            setupAction(options);

            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<RabbitPool>>();
            var pool = new RabbitPool(logger);

            foreach (var option in  options)
            {
                pool.RegisterConnection(option);
            }

            services.AddSingleton<IRabbitPool>(pool);

            return services;
        }
    }
}
