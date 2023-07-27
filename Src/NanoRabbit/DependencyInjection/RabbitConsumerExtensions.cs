using Microsoft.Extensions.DependencyInjection;
using NanoRabbit.Connection;
using NanoRabbit.Consumer;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitConsumerExtensions
    {
        /// <summary>
        /// RabbitConsumer Dependency Injection Functions.
        /// </summary>
        /// <typeparam name="TConsumer"></typeparam>
        /// <param name="services"></param>
        /// <param name="connectionName"></param>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsumer<TConsumer, TMessage>(this IServiceCollection services, string connectionName, string consumerName)
            where TConsumer : RabbitConsumer<TMessage>
        {
            services.AddSingleton<TConsumer>(provider =>
            {
                var consumer = ActivatorUtilities.CreateInstance<TConsumer>(provider, connectionName, consumerName, provider.GetRequiredService<IRabbitPool>());
                return consumer;
            });
            return services;
        }
    }
}
