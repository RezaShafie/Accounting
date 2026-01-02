namespace Accounting.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Voucher> Vouchers { get; }
    DbSet<VoucherLine> VoucherLines { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<TResult> ExecuteStrategyAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken);
}