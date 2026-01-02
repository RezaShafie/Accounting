namespace Accounting.ApiService.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case ValidationException validationEx:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Title = "خطای اعتبارسنجی";
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Detail = "خطای اعتبارسنجی رخ داد.";

                problemDetails.Extensions["errors"] = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;
            case ArgumentException or DomainException or NotFoundException:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Title = "خطای بیزنسی رخ داد";
                problemDetails.Detail = exception.Message;
                break;
            default:
                logger.LogError(exception, "خطای پیش بینی نشده رخ داد.");

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "خطای سرور";
                problemDetails.Detail = "خطای داخلی رخ داد، لطفا با راهبر سایت تماس بگیرید";
                break;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}