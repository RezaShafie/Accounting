namespace Accounting.Application.Common.Interfaces;

public interface IVoucherNumberGenerator
{
    Task<long> GetNextVoucherNumberAsync(int fiscalYear, CancellationToken ct);
}
