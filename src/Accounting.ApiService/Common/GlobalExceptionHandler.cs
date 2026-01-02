namespace Accounting.ApiService.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Result errorResponse;
        int statusCode;

        switch (exception)
        {
            // 1. Validation Errors (FluentValidation)
            case ValidationException validationEx:
                statusCode = StatusCodes.Status400BadRequest;

                // Flatten validation errors into a List<string> for the Result object
                var validationErrors = validationEx.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                errorResponse = Result.Failure("خطای اعتبارسنجی رخ داد.", validationErrors);
                break;

            // 2. Not Found Errors (Return 404)
            case NotFoundException or KeyNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                errorResponse = Result.Failure(exception.Message);
                break;

            // 3. Domain / Business Logic Errors (Return 400)
            case ArgumentException or DomainException:
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = Result.Failure(exception.Message);
                break;

            // 4. Unhandled Internal Server Errors (Return 500)
            default:
                logger.LogError(exception, "خطای پیش بینی نشده رخ داد.");

                statusCode = StatusCodes.Status500InternalServerError;
                errorResponse = Result.Failure("خطای داخلی رخ داد، لطفا با راهبر سایت تماس بگیرید");
                break;
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}