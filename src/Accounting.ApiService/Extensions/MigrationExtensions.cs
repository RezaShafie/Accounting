using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Accounting.ApiService.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        const int maxRetries = 10;
        const int delaySeconds = 5;

        logger.LogInformation("Starting database migration...");

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();

                logger.LogInformation("Database migration completed successfully.");
                return;
            }
            catch (SqlException ex) when (ex.Number == 1801)
            {
                logger.LogInformation("Database already exists. Continuing migration...");
            }
            catch (SqlException ex) when (IsTransient(ex))
            {
                if (attempt == maxRetries)
                {
                    logger.LogError(ex, "Database migration failed after {Attempts} attempts.", maxRetries);
                    throw;
                }

                logger.LogWarning(
                    "Database not ready (attempt {Attempt}/{Max}). Retrying in {Delay}s...",
                    attempt, maxRetries, delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during database migration.");
                throw;
            }
        }
    }

    private static bool IsTransient(SqlException ex)
        => ex.Number is
            -2      // Timeout
            or 53   // Network
            or 4060 // Cannot open database
            or 18456; // Login failed
}
