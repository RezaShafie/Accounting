namespace Accounting.Application.Features.Vouchers.DeleteVoucher;

public record DeleteVoucherCommand(Guid Id) : IRequest;
