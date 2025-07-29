using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace Fermat.Cqrs.Behaviors.Retries;

public class RetryBehavior<TRequest, TResponse>(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IRetryBehavior
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var retryCount = 3;
        var delayMs = 300;

        if (request is IRetryBehavior retryable)
        {
            retryCount = retryable.RetryCount;
            delayMs = retryable.RetryDelayMilliseconds;
        }

        var retryPolicy = Policy
            .Handle<Exception>(IsTransient)
            .WaitAndRetryAsync(retryCount,
                retryAttempt => TimeSpan.FromMilliseconds(delayMs),
                onRetry: (exception, timeSpan, retryNumber, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Retry {RetryNumber} encountered an error: {Message}. Waiting {Delay}ms before next retry.",
                        retryNumber,
                        exception.Message,
                        timeSpan.TotalMilliseconds);
                });

        return await retryPolicy.ExecuteAsync(() => next(cancellationToken));
    }

    private bool IsTransient(Exception ex)
    {
        return ex is TimeoutException || ex is HttpRequestException || ex.Message.Contains("transient");
    }
}