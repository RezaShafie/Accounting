using Accounting.ApiService.Common;

namespace Accounting.ApiService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. تنظیمات gRPC
        services.AddGrpc(options =>
        {
            options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
        });

        services.AddEndpointsApiExplorer();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddCustomHealthChecks(configuration);

        return services;
    }

    private static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("sqldata");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'sqldata' not found.");
        }

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "database_ef_core",
                tags: ["db", "ef"])
            .AddSqlServer(
                connectionString: connectionString,
                healthQuery: "SELECT 1;",
                name: "sql_server_instance",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                tags: ["db", "sql"]);

        return services;
    }
}