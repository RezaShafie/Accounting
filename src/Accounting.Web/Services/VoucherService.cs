using Accounting.Application.Features.Vouchers;
using Accounting.Shared.Models;
using Accounting.Shared.Requests;
using System.Net;

namespace Accounting.Web.Services;

public class VoucherService(HttpClient httpClient, ILogger<VoucherService> logger) : IVoucherService
{
    public async Task<PaginatedList<VoucherDto>> GetVouchersAsync(int pageNumber, int pageSize)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<PaginatedList<VoucherDto>>(
                $"api/vouchers?pageNumber={pageNumber}&pageSize={pageSize}");

            return result ?? new PaginatedList<VoucherDto>(new List<VoucherDto>(), 0, pageNumber, pageSize);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error fetching vouchers. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            throw new InvalidOperationException("Failed to retrieve vouchers from the server.", ex);
        }
    }

    public async Task<VoucherDto> GetVoucherByIdAsync(Guid id)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/vouchers/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Voucher with ID {id} was not found.");
            }

            response.EnsureSuccessStatusCode();

            var voucher = await response.Content.ReadFromJsonAsync<VoucherDto>();
            return voucher ?? throw new InvalidOperationException("Received null voucher from server.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode != HttpStatusCode.NotFound)
        {
            logger.LogError(ex, "Error fetching voucher with ID: {VoucherId}", id);
            throw new InvalidOperationException($"Failed to retrieve voucher with ID {id}.", ex);
        }
    }

    public async Task CreateVoucherAsync(CreateVoucherRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var response = await httpClient.PostAsJsonAsync("api/vouchers", request);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Bad request when creating voucher: {Error}", errorContent);
                throw new InvalidOperationException($"Invalid voucher data: {errorContent}");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error creating voucher");
            throw new InvalidOperationException("Failed to create voucher.", ex);
        }
    }

    public async Task UpdateVoucherAsync(Guid id, UpdateVoucherRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/vouchers/{id}", request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Voucher with ID {id} was not found.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Bad request when updating voucher {VoucherId}: {Error}", id, errorContent);
                throw new InvalidOperationException($"Invalid voucher data: {errorContent}");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex) when (ex.StatusCode != HttpStatusCode.NotFound)
        {
            logger.LogError(ex, "Error updating voucher with ID: {VoucherId}", id);
            throw new InvalidOperationException($"Failed to update voucher with ID {id}.", ex);
        }
    }

    public async Task DeleteVoucherAsync(Guid id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/vouchers/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Voucher with ID {id} was not found.");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex) when (ex.StatusCode != HttpStatusCode.NotFound)
        {
            logger.LogError(ex, "Error deleting voucher with ID: {VoucherId}", id);
            throw new InvalidOperationException($"Failed to delete voucher with ID {id}.", ex);
        }
    }
}