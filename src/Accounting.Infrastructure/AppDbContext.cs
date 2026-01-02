using Accounting.Application.Common.Interfaces;
using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<VoucherLine> VoucherLines { get; set; }
    public async Task BeginTransactionAsync()
    {
        await Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await Database.RollbackTransactionAsync();
    }

    public async Task<TResult> ExecuteStrategyAsync<TResult>(Func<Task<TResult>> operation, CancellationToken ct)
    {
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () => await operation());
    }

    public DbSet<VoucherSequence> VoucherSequences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}