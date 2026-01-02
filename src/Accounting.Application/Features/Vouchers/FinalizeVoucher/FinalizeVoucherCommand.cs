namespace Accounting.Application.Features.Vouchers.FinalizeVoucher;

public record FinalizeVoucherCommand(Guid Id) : IRequest;
