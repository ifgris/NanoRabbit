using Microsoft.Extensions.DependencyInjection;

namespace NanoRabbit.DependencyInjection
{
    /// <summary>
    /// Rabbit Handler Extensions
    /// </summary>
    public static class RabbitHandlerExtensions
    {
        /// <summary>
        /// Add Keyed Scoped Asynchronous Rabbit Handler
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TAsyncHandler"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddRabbitAsyncHandler<TAsyncHandler>(this IServiceCollection services)
            where TAsyncHandler : class, IAsyncMessageHandler
        {
            var serviceKey = typeof(TAsyncHandler).Name;
            services.AddKeyedScoped<IAsyncMessageHandler, TAsyncHandler>(serviceKey);
            return services;
        }
    }
}