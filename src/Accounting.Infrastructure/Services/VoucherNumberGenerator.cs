using Accounting.Application.Common.Interfaces;
using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.Services;

public class VoucherNumberGenerator(AppDbContext context) : IVoucherNumberGenerator
{
    public async Task<long> GetNextVoucherNumberAsync(int fiscalYear, CancellationToken ct)
    {
        // 1. Fetch with Lock
        // Since the Handler already started a transaction, this query automatically 
        // runs inside that transaction and respects the lock.
        var sequence = await context.VoucherSequences
            .FromSqlRaw("SELECT * FROM VoucherSequences WITH (UPDLOCK, ROWLOCK) WHERE FiscalYear = {0}", fiscalYear)
            .FirstOrDefaultAsync(ct);

        if (sequence == null)
        {
            // Case: First voucher of the year.
            // We just 'Add' to the context. We do NOT SaveChanges here.
            sequence = new VoucherSequence(fiscalYear);
            await context.VoucherSequences.AddAsync(sequence, ct);
        }

        // 2. Increment in Memory
        // The entity is now tracked. The Handler's final SaveChangesAsync will persist this update.
        return sequence.Increment();
    }
}