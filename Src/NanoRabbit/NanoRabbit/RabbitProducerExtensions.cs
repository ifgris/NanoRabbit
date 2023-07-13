using Microsoft.Extensions.DependencyInjection;

namespace NanoRabbit.NanoRabbit
{
    public static class RabbitProducerExtensions
    {
        public static IServiceCollection AddProducer<TProducer>(this IServiceCollection services, string connectionName, string producerName)
            where TProducer : class, IRabbitProducer
        {
            services.AddTransient<IRabbitProducer>(c =>
            {
                var pool = c.GetService<IRabbitPool>();
                var rabbitProducer = new RabbitProducer(connectionName, producerName, pool);
                return ActivatorUtilities.CreateInstance<TProducer>(c, rabbitProducer);
            });

            return services;
        }
    }
}
