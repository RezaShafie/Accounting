namespace Accounting.Application.Features.Vouchers.DeleteVoucher;

public class DeleteVoucherHandler(IAppDbContext context) : IRequestHandler<DeleteVoucherCommand>
{
    public async Task Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await context.Vouchers
            .FindAsync([request.Id], cancellationToken);

        if (voucher == null)
        {
            throw new NotFoundException($"سند با شناسه {request.Id} یافت نشد.");
        }

        if (voucher.IsFinalized)
        {
            throw new InvalidOperationException("امکان حذف یا ویرایش سند تایید شده وجود ندارد.");
        }

        context.Vouchers.Remove(voucher);
        await context.SaveChangesAsync(cancellationToken);
    }
}