using Accounting.Application.Common.Interfaces;
using Accounting.Domain.Entities;
using Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.Services;

public class VoucherNumberGenerator(AppDbContext context) : IVoucherNumberGenerator
{
    public async Task<long> GetNextVoucherNumberAsync(int fiscalYear, CancellationToken ct)
    {
        // 1. Create an execution strategy to handle SQL retries (resilience)
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // 2. Start a transaction
            using var transaction = await context.Database.BeginTransactionAsync(ct);
            try
            {
                // 3. FETCH WITH LOCK (The Secret Sauce)
                // 'TagWith' helps you find this query in Aspire Dashboard/Profiler
                // 'UPDLOCK' ensures other transactions wait if they try to read this row
                var sequence = await context.VoucherSequences
                    .FromSql($"SELECT * FROM VoucherSequences WITH (UPDLOCK, ROWLOCK) WHERE FiscalYear = {fiscalYear}")
                    .FirstOrDefaultAsync(ct);

                if (sequence == null)
                {
                    // If first voucher of the year, create the sequence record
                    sequence = new VoucherSequence(fiscalYear);
                    await context.VoucherSequences.AddAsync(sequence, ct);
                }

                // 4. Increment in memory
                var nextNumber = sequence.Increment();

                // 5. Save changes to DB (updates LastNumber)
                await context.SaveChangesAsync(ct);

                // 6. Commit the transaction
                await transaction.CommitAsync(ct);

                return nextNumber;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}