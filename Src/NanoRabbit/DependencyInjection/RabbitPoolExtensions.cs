using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NanoRabbit.Connection;

namespace NanoRabbit.DependencyInjection;

public static class RabbitPoolExtensions
{
    public static GlobalConfig? GlobalConfig;

    public static IServiceCollection AddRabbitPool(this IServiceCollection services,
        Action<GlobalConfig> setupGlobalConfig, Action<List<ConnectOptions>> setupAction)
    {
        GlobalConfig = new GlobalConfig();
        setupGlobalConfig(GlobalConfig);

        var options = new List<ConnectOptions>();
        setupAction(options);

        var pool = new RabbitPool(config => { config.EnableLogging = GlobalConfig.EnableLogging; });

        // ILogger<RabbitPool>? logger;
        // logger = GlobalConfig.EnableLogging ? services.BuildServiceProvider().GetRequiredService<ILogger<RabbitPool>>() : null;
        // var pool = new RabbitPool(logger);

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