using Accounting.Application.Features.Vouchers;
using Accounting.Shared.Models;
using Accounting.Shared.Requests;

namespace Accounting.Web.Services;

public class VoucherService(HttpClient httpClient, ILogger<VoucherService> logger) : IVoucherService
{
    private readonly System.Text.Json.JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PaginatedList<VoucherDto>> GetVouchersAsync(int pageNumber, int pageSize)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/vouchers?pageNumber={pageNumber}&pageSize={pageSize}");

            return await HandleResponseAsync<PaginatedList<VoucherDto>>(response)
                   ?? new PaginatedList<VoucherDto>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در واکشی اسناد");
            throw;
        }
    }

    public async Task<VoucherDto> GetVoucherByIdAsync(Guid id)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/vouchers/{id}");
            return await HandleResponseAsync<VoucherDto>(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در واکشی سند {Id}", id);
            throw;
        }
    }

    public async Task CreateVoucherAsync(CreateVoucherRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/vouchers", request);
            await HandleResponseAsync<Guid>(response); // We might not use the Guid here, but we check for success
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در ساخت سند");
            throw;
        }
    }

    public async Task UpdateVoucherAsync(Guid id, UpdateVoucherRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/vouchers/{id}", request);
            await HandleResponseAsync(response); // Non-generic version for void returns
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در بروزرسانی {Id}", id);
            throw;
        }
    }

    public async Task DeleteVoucherAsync(Guid id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/vouchers/{id}");
            await HandleResponseAsync(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در حذف سند {Id}", id);
            throw;
        }
    }



    /// <summary>
    /// Handles response for endpoints returning data (Result<T>).
    /// </summary>
    private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<Result<T>>(_options);

        if (result is null)
        {
            throw new InvalidOperationException("جوابی یافت نشد");
        }


        if (!result.IsSuccess)
        {
            throw new Exception(BuildErrorMessage(result.Message, result.Errors));
        }

        // 3. Return Data
        return result.Data!;
    }

    /// <summary>
    /// Handles response for void endpoints (Result).
    /// </summary>
    private async Task HandleResponseAsync(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<Result>(_options);

        if (result is null)
        {
            throw new InvalidOperationException("API returned an empty response.");
        }

        if (!result.IsSuccess)
        {
            throw new Exception(BuildErrorMessage(result.Message, result.Errors));
        }
    }

    private static string BuildErrorMessage(string? message, List<string>? errors)
    {
        if (errors is null || errors.Count == 0)
        {
            return message ?? "خطای نامشخص رخ داد";
        }

        return $"{message} ({string.Join(", ", errors)})";
    }
}