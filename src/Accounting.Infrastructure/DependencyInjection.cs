using Accounting.Application.Common.Interfaces;
using Accounting.Infrastructure.Persistence.Interceptors;
using Accounting.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Accounting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IVoucherNumberGenerator, VoucherNumberGenerator>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddScoped<IAppDbContext>(provider =>
            provider.GetRequiredService<AppDbContext>());

        return services;
    }

    public static void EnrichInfrastructureDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<AppDbContext>("sqldata", configureDbContextOptions: options =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var auditableInterceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
            options.AddInterceptors(auditableInterceptor);
        });
    }
}