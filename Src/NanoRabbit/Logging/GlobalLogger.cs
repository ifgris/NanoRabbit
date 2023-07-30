using Microsoft.Extensions.Logging;

namespace NanoRabbit.Logging
{
    public static class GlobalLogger
    {
        // private static ILoggerFactory Factory { get; } = new LoggerFactory();
        //
        // public static ILogger<T> CreateLogger<T>()
        // {
        //     return Factory.CreateLogger<T>();
        // }
        private static ILoggerFactory Factory { get; } = InitializeLoggerFactory();

        private static ILoggerFactory InitializeLoggerFactory() 
        {
            var loggerFactory = LoggerFactory.Create(builder => {
                builder.AddConsole();
            });
            return loggerFactory;
        }  

        public static ILogger<T> CreateLogger<T>() 
        {
            return Factory.CreateLogger<T>();
        }
    }
}
