using Accounting.Application.Common.Mappings;
using Accounting.Application.Features.Vouchers.UpdateVoucher;
using Accounting.Shared.Models;

namespace Accounting.Application.Features.Vouchers.GetVouchers;

public class GetVouchersHandler(IAppDbContext context) : IRequestHandler<GetVouchersQuery, PaginatedList<VoucherDto>>
{
    public async Task<PaginatedList<VoucherDto>> Handle(GetVouchersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Vouchers
            .Include(v => v.Lines)
            .AsNoTracking()
            .OrderByDescending(v => v.Date)
            .ThenByDescending(v => v.VoucherNumber)
            .Select(v => new VoucherDto(
                v.Id,
                v.VoucherNumber,
                v.Date,
                v.Description,
                v.IsFinalized,
                v.Lines.Select(l => new VoucherLineDto(
                    l.AccountCoding.Value,
                    l.Description,
                    l.Debit,
                    l.Credit
                )).ToList()
            ));

        return await query
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
