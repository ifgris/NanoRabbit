using Microsoft.Extensions.Logging;

namespace NanoRabbit.Logging;

/// <summary>
/// GlobalLogger Static Class, which is used to create Logger simply.
/// </summary>
public static class GlobalLogger
{
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;

    public static void ConfigureLogging()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);

            // Add default console logging
            builder.AddConsole();
        });

        _logger = _loggerFactory.CreateLogger("RabbitHelper");
    }

    public static ILogger? Logger => _logger;
}