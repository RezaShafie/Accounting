using Accounting.Application.Features.Vouchers;

namespace Accounting.Shared.Requests;

public class CreateVoucherRequest
{
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public List<VoucherLineDto> Lines { get; set; } = [];
}

public class UpdateVoucherRequest
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public List<VoucherLineDto> Lines { get; set; } = [];
}