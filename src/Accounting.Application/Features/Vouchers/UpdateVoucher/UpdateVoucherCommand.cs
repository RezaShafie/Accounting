namespace Accounting.Application.Features.Vouchers.UpdateVoucher;

public record UpdateVoucherCommand(
    Guid Id,
    string? Description,
    DateOnly Date,
    List<VoucherLineDto> Lines 
) : IRequest;