using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfProcess.Data.Configs;
using PdfProcess.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;

namespace PdfProcess.RabbitMq;

[ExcludeFromCodeCoverage]
public static class RabbitRegistry
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqConfig>(options =>
        {
            configuration.GetSection(nameof(RabbitMqConfig)).Bind(options);
        });

        services.AddSingleton<Task<IConnection>>(async sp =>
        {
            var options = sp.GetRequiredService<IOptions<RabbitMqConfig>>();
            var rabbitConfig = options.Value;

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig.HostName,
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                VirtualHost = rabbitConfig.VirtualHost,
                AutomaticRecoveryEnabled = true
            };

            return await factory.CreateConnectionAsync();
        });

        services.AddSingleton<IRabbitMqService>(sp =>
        {
            var connectionTask = sp.GetRequiredService<Task<IConnection>>();
            var connection = connectionTask.GetAwaiter().GetResult();
            var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();

            return new RabbitMqService(connection, logger);
        });

        return services;
    }

}
