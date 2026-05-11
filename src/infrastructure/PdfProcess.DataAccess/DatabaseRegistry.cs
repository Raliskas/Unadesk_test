using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace PdfProcess.DataAccess;

[ExcludeFromCodeCoverage]
public static class DatabaseRegistry
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection");
        services.AddDbContext<DocumentContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}