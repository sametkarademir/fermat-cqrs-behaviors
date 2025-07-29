using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fermat.Cqrs.Behaviors.Logs;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ILoggingBehavior
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("➡️ Handling {RequestName} with payload: {@Request}", requestName, request);

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            logger.LogInformation("✅ Handled {RequestName} in {ElapsedMilliseconds}ms. Response: {@Response}",
                requestName, stopwatch.ElapsedMilliseconds, response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "❌ Error handling {RequestName} after {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}