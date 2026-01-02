namespace Accounting.Application.Features.Vouchers.GetVoucherById;

public class GetVoucherByIdHandler(IAppDbContext context) : IRequestHandler<GetVoucherByIdQuery, VoucherDto>
{
    public async Task<VoucherDto> Handle(GetVoucherByIdQuery request, CancellationToken cancellationToken)
    {
        var voucher = await context.Vouchers
            .Include(v => v.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
        {
       
            throw new NotFoundException($"سند با شناسه {request.Id} یافت نشد.");
        }

  
        return new VoucherDto(
            voucher.Id,
            voucher.VoucherNumber,
            voucher.Date,
            voucher.Description,
            voucher.IsFinalized,
            voucher.Lines.Select(l => new VoucherLineDto(
                l.AccountCoding.Value,
                l.Description,
                l.Debit,
                l.Credit
            )).ToList()
        );
    }
}