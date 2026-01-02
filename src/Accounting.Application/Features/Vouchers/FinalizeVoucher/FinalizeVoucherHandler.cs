namespace Accounting.Application.Features.Vouchers.FinalizeVoucher;

public class FinalizeVoucherHandler(IAppDbContext context) : IRequestHandler<FinalizeVoucherCommand>
{
    public async Task Handle(FinalizeVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await context.Vouchers.Include(v => v.Lines)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher is null)
        {
            throw new NotFoundException($"سند با شناسه {request.Id} یافت نشد.");
        }

        if (!voucher.IsBalanced())
        {
            throw new ArgumentException("سند غیرتراز نمیتواند نهایی شود");
        }

        voucher.FinalizeVoucher();

        await context.SaveChangesAsync(cancellationToken);
    }
}