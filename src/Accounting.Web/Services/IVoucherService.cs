using Accounting.Application.Features.Vouchers;
using Accounting.Shared.Models;
using Accounting.Shared.Requests;

namespace Accounting.Web.Services;

public interface IVoucherService
{
    Task<PaginatedList<VoucherDto>> GetVouchersAsync(int pageNumber, int pageSize);
    Task<VoucherDto> GetVoucherByIdAsync(Guid id);
    Task CreateVoucherAsync(CreateVoucherRequest request);
    Task UpdateVoucherAsync(Guid id, UpdateVoucherRequest request);
    Task DeleteVoucherAsync(Guid id);
}
