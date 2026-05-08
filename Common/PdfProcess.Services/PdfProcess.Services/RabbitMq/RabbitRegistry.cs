using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfProcess.Services.Interfaces;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;

namespace PdfProcess.Services.RabbitMq;

[ExcludeFromCodeCoverage]
public static class RabbitRegistry
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        //TODO мог вынести в конфиги, но уже лень стало 
        var rabbitMqConfig = configuration.GetSection("RabbitMq");

        var hostName = rabbitMqConfig["HostName"];
        var port = int.Parse(rabbitMqConfig["Port"] ?? "5672");
        var userName = rabbitMqConfig["UserName"];
        var password = rabbitMqConfig["Password"];
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                VirtualHost = virtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            return factory.CreateConnection();
        });

        services.AddScoped<IRabbitMqService, RabbitMqService>();

        return services;
    }

}
