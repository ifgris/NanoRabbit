using Microsoft.Extensions.DependencyInjection;
using NanoRabbit.Connection;
using NanoRabbit.Producer;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitProducerExtensions
    {
        /// <summary>
        /// RabbitProducer Dependency Injection Functions.
        /// </summary>
        /// <typeparam name="TProducer"></typeparam>
        /// <param name="services"></param>
        /// <param name="connectionName"></param>
        /// <param name="producerName"></param>
        /// <returns></returns>
        public static IServiceCollection AddProducer<TProducer>(this IServiceCollection services, string connectionName, string producerName)
            where TProducer : RabbitProducer
        {
            services.AddSingleton<TProducer>(provider => ActivatorUtilities.CreateInstance<TProducer>(provider, connectionName, producerName, provider.GetRequiredService<IRabbitPool>()));
            return services;
        }
    }
}
