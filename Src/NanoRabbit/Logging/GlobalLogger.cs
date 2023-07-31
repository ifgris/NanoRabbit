using Microsoft.Extensions.Logging;

namespace NanoRabbit.Logging
{
    /// <summary>
    /// GlobalLogger Static Class, used to create Logger simply.
    /// </summary>
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

        /// <summary>
        /// Create Logger with type param
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger<T> CreateLogger<T>() 
        {
            return Factory.CreateLogger<T>();
        }
    }
}
