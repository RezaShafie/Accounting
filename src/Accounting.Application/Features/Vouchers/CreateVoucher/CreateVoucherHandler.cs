namespace Accounting.Application.Features.Vouchers.CreateVoucher;

public class CreateVoucherHandler(IAppDbContext context, IVoucherNumberGenerator numberGenerator) : IRequestHandler<CreateVoucherCommand, Guid>
{
    public async Task<Guid> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        using var transaction = context.BeginTransactionAsync();

        try
        {
            var fiscalYear = request.Date.Year;
            var voucherNumber = await numberGenerator.GetNextVoucherNumberAsync(fiscalYear, cancellationToken);

            var voucher = Voucher.Create(voucherNumber, request.Description, request.Date);

            foreach (var line in request.Lines)
            {
                voucher.AddLine(line.AccountCode, line.Description, line.Debit, line.Credit);
            }

            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync(cancellationToken);

            await context.CommitTransactionAsync();

            return voucher.Id;
        }
        catch (Exception e)
        {
            await context.RollbackTransactionAsync();
            throw;
        }
        
    }
}