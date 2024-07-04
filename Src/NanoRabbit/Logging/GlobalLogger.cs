using Serilog;

namespace NanoRabbit.Logging;

/// <summary>
/// GlobalLogger Static Class, used to create Logger simply.
/// </summary>
public static class GlobalLogger
{
    private static ILogger _logger;

    public static bool IsLoggingEnabled { get; private set; }

    public static void ConfigureLogging(bool enableLogging)
    {
        IsLoggingEnabled = enableLogging;

        if (enableLogging)
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
        else
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Fatal))
                .CreateLogger();
        }
    }

    public static ILogger Logger => _logger;
}