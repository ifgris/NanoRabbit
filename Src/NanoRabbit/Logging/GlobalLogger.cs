using Microsoft.Extensions.Logging;

namespace NanoRabbit.Logging
{
    public static class GlobalLogger
    {
        public static ILoggerFactory Factory { get; } = new LoggerFactory();

        public static ILogger<T> CreateLogger<T>()
        {
            return Factory.CreateLogger<T>();
        }
    }
}
