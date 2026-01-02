namespace Accounting.Application.Features.Vouchers.UpdateVoucher;

public class UpdateVoucherHandler(IAppDbContext context) : IRequestHandler<UpdateVoucherCommand>
{

    public async Task Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await context.Vouchers
            .Include(v => v.Lines)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
        {
            throw new NotFoundException($"سند با شناسه {request.Id} یافت نشد.");
        }

        voucher.Update(request.Description, request.Date);


        voucher.ClearLines();

        foreach (var lineDto in request.Lines)
        {
            voucher.AddLine(
                lineDto.AccountCode,
                lineDto.Description,
                lineDto.Debit,
                lineDto.Credit
            );
        }

        if (!voucher.IsBalanced())
        {
            throw new InvalidOperationException("سند تراز نیست.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}