namespace Accounting.Application.Features.Vouchers.FinalizeVoucher;

public class FinalizeVoucherHandler(IAppDbContext context) : IRequestHandler<FinalizeVoucherCommand>
{
    public async Task Handle(FinalizeVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await context.Vouchers
            .FindAsync([request.Id], cancellationToken);

        if (voucher is null)
        {
            throw new NotFoundException($"سند با شناسه {request.Id} یافت نشد.");
        }

        voucher.FinalizeVoucher();

        await context.SaveChangesAsync(cancellationToken);
    }
}