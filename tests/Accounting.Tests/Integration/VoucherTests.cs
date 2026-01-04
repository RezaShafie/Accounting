using Accounting.Application.Features.Vouchers;
using Accounting.Shared.Requests;
using Aspire.Hosting;
using FluentAssertions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Accounting.Shared.Models;

namespace Accounting.Tests.Integration;

public class VoucherTests(ITestOutputHelper output) : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(3);
    private DistributedApplication _app;
    private HttpClient _httpClient;
    private const string ApiResourceName = "apiservice";



    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Accounting_AppHost>(cancellationToken);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter("Aspire", LogLevel.Debug);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });


        _app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await _app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync(ApiResourceName, cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        _httpClient = _app.CreateHttpClient(ApiResourceName);
    }

    // ==========================================
    // Happy Paths (Standard CRUD)
    // ==========================================

    [Fact]
    public async Task CreateVoucher_WithBalancedLines_ReturnsSuccessResult()
    {
        // Arrange
        var request = VoucherTestFactory.CreateBalancedRequest();

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/vouchers", request, cancellationToken: TestContext.Current.CancellationToken);

        var jsonString = await response.Content.ReadAsStringAsync();

        // چاپ در خروجی تست
        output.WriteLine(">>> JSON RESPONSE: " + jsonString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // تغییر از 201 به 200 طبق اندپوینت

        // Deserialize Result<Guid>
        var result = await response.Content.ReadFromJsonAsync<Result<string>>();

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue("Response should indicate success");
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetVoucher_ById_ReturnsCorrectData()
    {
        // Arrange
        var createdId = await CreateVoucherAndGetIdAsync();

        // Act
        var response = await _httpClient.GetAsync($"/api/vouchers/{createdId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<VoucherDto>>();

        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(createdId);
        result.Data.Lines.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateVoucher_WithValidChanges_ReturnsSuccess()
    {
        // Arrange
        var createdId = await CreateVoucherAndGetIdAsync();

        // دریافت دیتای فعلی برای داشتن لاین‌ها (چون لاین‌ها الزامی هستند)
        var getResponse = await _httpClient.GetAsync($"/api/vouchers/{createdId}");
        var currentVoucher = (await getResponse.Content.ReadFromJsonAsync<Result<VoucherDto>>())!.Data;

        var updateRequest = new UpdateVoucherRequest
        {
            Id = createdId,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
            Description = "Updated Description",
            Lines = currentVoucher.Lines
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/vouchers/{createdId}",
            updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result>();
        result!.IsSuccess.Should().BeTrue();

        // Verify Update
        var verifyResponse = await _httpClient.GetAsync($"/api/vouchers/{createdId}");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<Result<VoucherDto>>();
        verifyResult!.Data.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteVoucher_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var createdId = await CreateVoucherAndGetIdAsync();

        // Act
        var response = await _httpClient.DeleteAsync($"/api/vouchers/{createdId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result>();
        result!.IsSuccess.Should().BeTrue();

        // Verify Deletion (Should return 404 because of NotFoundException)
        var checkResponse = await _httpClient.GetAsync($"/api/vouchers/{createdId}");
        checkResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FinalizeVoucher_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var createdId = await CreateVoucherAndGetIdAsync();

        // Act
        // طبق اندپوینت: POST api/vouchers/finalize/{id}
        var response = await _httpClient.PostAsync(
            $"/api/vouchers/finalize/{createdId}",
            null); // Empty body

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result>();
        result!.IsSuccess.Should().BeTrue();
    }

    // ==========================================
    // Validation & Logic Failures
    // ==========================================

    [Fact]
    public async Task UpdateVoucher_IdMismatch_ReturnsFailureResult()
    {
        // Arrange
        // در این سناریو طبق کد کنترلر، وضعیت 200 برمی‌گردد اما Result.Failure است
        var createdId = await CreateVoucherAndGetIdAsync();
        var updateRequest = new UpdateVoucherRequest
        {
            Id = Guid.NewGuid(), // Mismatch ID
            Lines = new List<VoucherLineDto>()
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/vouchers/{createdId}",
            updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<Result>();
        result!.IsSuccess.Should().BeFalse("Should fail due to ID mismatch");
    }

    [Fact]
    public async Task CreateVoucher_UnbalancedLines_ReturnsBadRequest()
    {
        // Arrange
        // معمولاً FluentValidation قبل از هندلر اجرا می‌شود و ValidationException می‌دهد
        // که تبدیل به 400 می‌شود. اگر داخل هندلر هندل شده باشد ممکن است Result.Failure بدهد.
        // فرض بر استاندارد Aspire/CleanArch: Validation Exception -> 400
        var request = VoucherTestFactory.CreateUnbalancedRequest();

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/vouchers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVoucher_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _httpClient.GetAsync($"/api/vouchers/{nonExistentId}");

        // Assert
        // چون کنترلر NotFoundException پرتاب می‌کند، میدلور باید 404 برگرداند
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateVoucher_WhenFinalized_ReturnsFailure()
    {
        // Arrange
        var id = await CreateVoucherAndGetIdAsync();

        // 1. Finalize
        var finalizeRes = await _httpClient.PostAsync($"/api/vouchers/finalize/{id}", null);
        finalizeRes.EnsureSuccessStatusCode();

        // 2. Try Update
        var getResponse = await _httpClient.GetAsync($"/api/vouchers/{id}");
        var dto = (await getResponse.Content.ReadFromJsonAsync<Result<VoucherDto>>())!.Data;

        var updateRequest = new UpdateVoucherRequest
        {
            Id = id,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Description = "Trying to edit finalized voucher",
            Lines = dto.Lines
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/vouchers/{id}", updateRequest);

        // Assert
        // بسته به اینکه لاجیک Finalize در Validator است (400) یا در Handler (Result.Failure/200)
        // معمولاً بیزنس رول‌ها در هندلر چک می‌شوند:
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<Result>();
            result!.IsSuccess.Should().BeFalse("Should not allow update on finalized voucher");
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
    }

    // --- Helpers ---

    private async Task<Guid> CreateVoucherAndGetIdAsync()
    {
        var request = VoucherTestFactory.CreateBalancedRequest();
        var response = await _httpClient.PostAsJsonAsync("/api/vouchers", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
        if (!result!.IsSuccess)
            throw new Exception($"Helper creation failed: {result.Errors}");

        return result.Data;
    }
}

// ==========================================
// Mock Result Classes (For Test Deserialization)
// ==========================================
// این کلاس‌ها برای مپ کردن خروجی API در تست‌ها استفاده می‌شوند
public class Result
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; } // یا Message بسته به پیاده‌سازی شما
}


// ==========================================
// Test Data Factory
// ==========================================
public static class VoucherTestFactory
{
    public static CreateVoucherRequest CreateBalancedRequest()
    {
        return new CreateVoucherRequest
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Description = "Integration Test Voucher",
            Lines = new List<VoucherLineDto>
            {
                new() { AccountCode = "1000-1000-1000", Description = "D", Debit = 1000, Credit = 0 },
                new() { AccountCode = "2000-2000-2000", Description = "C", Debit = 0, Credit = 1000 }
            }
        };
    }

    public static CreateVoucherRequest CreateUnbalancedRequest()
    {
        var r = CreateBalancedRequest();
        r.Lines[1].Credit = 500;
        return r;
    }
}