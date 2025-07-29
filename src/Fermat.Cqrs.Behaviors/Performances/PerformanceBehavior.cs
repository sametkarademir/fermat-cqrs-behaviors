using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fermat.Cqrs.Behaviors.Performances;

public interface IPerformanceBehavior
{
    int ThresholdMilliseconds => 500;
}

public class PerformanceBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>, IPerformanceBehavior
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var thresholdMilliseconds = 500;
        
        if (request is IPerformanceBehavior performanceBehavior)
        {
            thresholdMilliseconds = performanceBehavior.ThresholdMilliseconds;
        }
        
        
        _timer.Start();

        var response = await next(cancellationToken);

        _timer.Stop();

        var elapsedMs = _timer.ElapsedMilliseconds;

        if (elapsedMs > thresholdMilliseconds)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "🐢 Long Running Request: {RequestName} took {ElapsedMilliseconds}ms. Payload: {@Request}",
                requestName, elapsedMs, request);
        }

        return response;
    }
}