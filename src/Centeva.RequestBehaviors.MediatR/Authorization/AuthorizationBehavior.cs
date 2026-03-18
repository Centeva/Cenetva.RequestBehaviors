using System.Collections.Concurrent;
using System.Reflection;
using Ardalis.Result;
using Centeva.RequestBehaviors.Common.Authorization;
using MediatR;

namespace Centeva.RequestBehaviors.MediatR.Authorization;

/// <summary>
/// Pipeline behavior that authorizes requests.
/// For Result and Result&lt;T&gt; responses, unauthorized requests are returned as Forbidden results.
/// For other response types, throws ValidationException on failure.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IRequestAuthorizer<TRequest>> _authorizers;

    private static readonly ConcurrentDictionary<Type, Type> RequirementHandlers = new();

    private static readonly ConcurrentDictionary<Type, MethodInfo> HandlerMethodInfo = new();

    private static readonly ConcurrentDictionary<Type, MethodInfo> ForbiddenMethodInfo = new();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the AuthorizationBehavior class with the specified authorizers and service
    /// provider.
    /// </summary>
    /// <param name="authorizers">A collection of request authorizers to be used for authorizing requests of type TRequest. Cannot be null.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required by the authorization behavior. Cannot be null.</param>
    public AuthorizationBehavior(IEnumerable<IRequestAuthorizer<TRequest>> authorizers,
        IServiceProvider serviceProvider)
    {
        _authorizers = authorizers;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Processes the specified request by evaluating authorization requirements and invoking the next handler in the
    /// pipeline if authorization succeeds.
    /// </summary>
    /// <param name="request">The request message to be handled. This object is evaluated against authorization requirements before processing
    /// continues.</param>
    /// <param name="next">A delegate that, when invoked, calls the next handler in the request pipeline and returns a response.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response returned by the next
    /// handler if authorization is successful; otherwise, an unauthorized response.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requirements = new HashSet<IRequestAuthorizationRequirement>();

        foreach (var authorizer in _authorizers)
        {
            authorizer.BuildPolicy(request);
            foreach (var requirement in authorizer.Requirements)
                requirements.Add(requirement);
        }

        foreach (var requirement in requirements)
        {
            var result = await ExecuteAuthorizationHandler(requirement, cancellationToken);

            if (!result.IsAuthorized)
            {
                return HandleUnauthorizedResult(result);
            }
        }

        return await next();
    }

    private TResponse HandleUnauthorizedResult(AuthorizationResult result)
    {
        var responseType = typeof(TResponse);
        var failureMessage = result.FailureMessage ?? "Unauthorized";

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Forbidden([failureMessage]);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = responseType.GetGenericArguments()[0];

            var forbiddenMethod = ForbiddenMethodInfo.GetOrAdd(resultType, valueType =>
            {
                var genericResultType = typeof(Result<>).MakeGenericType(valueType);
                var method = genericResultType.GetMethod(
                    nameof(Result.Forbidden),
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(string[])],
                    null);

                return method ?? throw new InvalidOperationException(
                    $"Could not find Result<{valueType.Name}>.Forbidden(string[]) method. " +
                    "This may indicate an incompatible version of Ardalis.Result.");
            });

            var forbiddenResult = forbiddenMethod.Invoke(null, [new[] { failureMessage }]);
            return (TResponse)forbiddenResult!;
        }

        throw new NotAuthorizedException(failureMessage);
    }

    private Task<AuthorizationResult> ExecuteAuthorizationHandler(IRequestAuthorizationRequirement requirement,
        CancellationToken cancellationToken)
    {
        Type requirementType = requirement.GetType();
        Type requirementHandlerType = GetRequirementHandlerType(requirement);

#pragma warning disable IDE0019 // Use pattern matching
        IEnumerable<object>? handlers =
            _serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(requirementHandlerType)) as IEnumerable<object>;
#pragma warning restore IDE0019 // Use pattern matching

        if (handlers == null || !handlers.Any())
            throw new InvalidOperationException(
                $"Could not find an authorization handler implementation for requirement type \"{requirementType.Name}\"");

        if (handlers.Count() > 1)
            throw new InvalidOperationException(
                $"Multiple authorization handler implementations were found for requirement type \"{requirementType.Name}\"");

        var requirementHandlerToUse = handlers.First();
        var requirementHandlerToUseType = requirementHandlerToUse.GetType();

        var handleMethod = HandlerMethodInfo.GetOrAdd(requirementHandlerToUseType,
            handlerMethodKey => requirementHandlerToUseType
                .GetMethods()
                .FirstOrDefault(x => x.Name == nameof(IRequestAuthorizationHandler<IRequestAuthorizationRequirement>.Handle))!);

        // Reflection above ensures that these warnings aren't relevant
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
        return (Task<AuthorizationResult>)handleMethod.Invoke(requirementHandlerToUse,
            [requirement, cancellationToken]);
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }

    private static Type GetRequirementHandlerType(IRequestAuthorizationRequirement requirement)
    {
        var requirementType = requirement.GetType();
        var handlerType = RequirementHandlers.GetOrAdd(requirementType,
            requirementTypeKey => typeof(IRequestAuthorizationHandler<>).MakeGenericType(requirementTypeKey));

        return handlerType;
    }
}
