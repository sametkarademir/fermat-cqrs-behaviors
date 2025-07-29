namespace Fermat.Cqrs.Behaviors.Retries;

public interface IRetryBehavior
{
    int RetryCount => 3;
    int RetryDelayMilliseconds => 500;
}