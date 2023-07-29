using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitPoolExtensions
    {
        public static GlobalConfig? _globalConfig;
        public static IServiceCollection AddRabbitPool(this IServiceCollection services, Action<GlobalConfig> setupGlobalConfig, Action<List<ConnectOptions>> setupAction)
        {
            _globalConfig = new GlobalConfig();
            setupGlobalConfig(_globalConfig);

            var options = new List<ConnectOptions>();
            setupAction(options);

            //var pool = new RabbitPool(config =>
            //{
            //    config.EnableLogging = globalConfig.EnableLogging;
            //});

            ILogger<RabbitPool>? logger;
            if (_globalConfig.EnableLogging)
            {
                logger = services.BuildServiceProvider().GetRequiredService<ILogger<RabbitPool>>();
            }
            else
            {
                logger = null;
            }
            var pool = new RabbitPool(logger);

            //var pool = new RabbitPool(logger, config =>
            //{
            //    config.EnableLogging = globalConfig.EnableLogging;
            //});

            foreach (var option in options)
            {
                pool.RegisterConnection(option);
            }

            services.AddSingleton<IRabbitPool>(pool);

            return services;
        }
    }
}
