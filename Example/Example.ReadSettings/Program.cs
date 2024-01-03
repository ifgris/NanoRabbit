using NanoRabbit.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Example.ReadSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

try
{
    var host = CreateHostBuilder(args).Build();
    await host.RunAsync();
}
catch (Exception)
{
    throw;
}

IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>((context, builders) =>
    {
        // ...
    })
    .ConfigureServices((context, services) =>
    {
        services.AutoAddRabbitProducer(context.Configuration);
        services.AutoAddRabbitConsumer(context.Configuration);

        // register BackgroundService
        services.AddHostedService<PublishService>();
        services.AddHostedService<ConsumeService>();
    });