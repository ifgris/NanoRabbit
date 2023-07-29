using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services.AddSingleton<TProducer>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitProducer>>();
                var producer = ActivatorUtilities.CreateInstance<TProducer>(provider, connectionName, producerName, provider.GetRequiredService<IRabbitPool>(), logger);
                return producer;
            });

            return services;
        }
    }
}
