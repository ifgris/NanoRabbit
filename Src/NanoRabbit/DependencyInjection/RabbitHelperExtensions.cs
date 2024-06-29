using Microsoft.Extensions.DependencyInjection;
using NanoRabbit.Helper;
using NanoRabbit.Helper.MessageHandler;

namespace NanoRabbit.DependencyInjection
{
    public static class RabbitHelperExtensions
    {
        public static IServiceCollection AddRabbitMqHelper(this IServiceCollection services, string hostname,
            int port = 5672,
            string virtualHost = "/",
            string userName = "guest",
            string password = "guest")
        {
            services.AddSingleton<IRabbitHelper>(sp => new RabbitHelper(hostname, port, virtualHost, userName, password));
            return services;
        }

        public static IServiceCollection AddRabbitConsumer<THandler>(this IServiceCollection services, string queueName, int consumers = 1)
            where THandler : class, IMessageHandler
        {
            services.AddSingleton<THandler>();

            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqHelper = serviceProvider.GetRequiredService<IRabbitHelper>();
            var messageHandler = serviceProvider.GetRequiredService<THandler>();

            // todo: check if queue exists.
            // rabbitMqHelper.DeclareQueue(queueName);

            rabbitMqHelper.AddConsumer(queueName, messageHandler.HandleMessage, consumers);

            return services;
        }
    }
}
