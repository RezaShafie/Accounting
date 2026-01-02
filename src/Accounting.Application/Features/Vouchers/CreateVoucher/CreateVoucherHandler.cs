namespace Accounting.Application.Features.Vouchers.CreateVoucher;

public class CreateVoucherHandler(IAppDbContext context) : IRequestHandler<CreateVoucherCommand, Guid>
{
    public async Task<Guid> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = Voucher.Create(request.VoucherNumber, request.Description, request.Date);

        foreach (var line in request.Lines)
        {
            voucher.AddLine(line.AccountCode, line.Description, line.Debit, line.Credit);
        }

        context.Vouchers.Add(voucher);
        await context.SaveChangesAsync(cancellationToken);

        return voucher.Id;
    }
}