namespace Accounting.Application.Features.Vouchers.CreateVoucher;

public sealed record CreateVoucherCommand(
    string Description,
    DateOnly Date,
    List<VoucherLineDto> Lines
) : IRequest<Guid>;
