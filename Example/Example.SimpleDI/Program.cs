using Example.SimpleDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NanoRabbit.NanoRabbit;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitPool(new Dictionary<string, ConnectOptions>
    {
        {"Connection1", new ConnectOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "admin",
                VirtualHost = "DATA"
            }
        },
        {"Connection2", new ConnectOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "admin",
                VirtualHost = "HOST"
            }
        }
    });

builder.Services.AddHostedService<PublishService>();
using IHost host = builder.Build();

await host.RunAsync();
