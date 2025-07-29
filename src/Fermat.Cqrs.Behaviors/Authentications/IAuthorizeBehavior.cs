using Fermat.Domain.Exceptions.Types;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fermat.Cqrs.Behaviors.Authentications;

public interface IAuthorizeBehavior
{
    IReadOnlyCollection<string> RequiredRoles { get; }
}

public class AuthorizationBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>, IAuthorizeBehavior
{
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizeBehavior authorize)
        {
            if (!httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                throw new AppAuthenticationException();
            }

            var hasAnyRole = authorize.RequiredRoles
                .Any(role => httpContextAccessor.HttpContext.User.IsInRole(role));

            if (!hasAnyRole)
            {
                throw new AppAuthorizationException();
            }
        }

        return await next(cancellationToken);
    }
}