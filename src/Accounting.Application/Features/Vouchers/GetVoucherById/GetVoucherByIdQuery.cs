namespace Accounting.Application.Features.Vouchers.GetVoucherById;

public record GetVoucherByIdQuery(Guid Id) : IRequest<VoucherDto>;