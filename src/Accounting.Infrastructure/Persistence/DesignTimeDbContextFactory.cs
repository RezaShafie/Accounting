using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Accounting.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Accounting.Api");

        if (!Directory.Exists(basePath))
        {
            basePath = Path.Combine(Directory.GetCurrentDirectory(), "src/Accounting.Api");
        }


        var builder = new DbContextOptionsBuilder<AppDbContext>();

        builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AccountingDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new AppDbContext(builder.Options);
    }
}