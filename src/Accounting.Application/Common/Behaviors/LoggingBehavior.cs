namespace Accounting.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Starting Request: {Name} {@Request}", requestName, request);

        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next();

            timer.Stop();

            if (timer.ElapsedMilliseconds > 500)
            {
                logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)",
                    requestName, timer.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation("Completed Request: {Name} in {ElapsedMilliseconds} ms",
                    requestName, timer.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request Failure: {Name} {@Request}", requestName, request);
            throw;
        }
    }
}