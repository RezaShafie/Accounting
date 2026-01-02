using Accounting.Shared.Models;

namespace Accounting.Application.Features.Vouchers.GetVouchers;

public record GetVouchersQuery : IRequest<PaginatedList<VoucherDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}