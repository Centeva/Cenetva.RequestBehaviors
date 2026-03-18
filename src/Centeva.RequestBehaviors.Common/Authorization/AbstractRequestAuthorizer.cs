namespace Centeva.RequestBehaviors.Common.Authorization;

/// <summary>
/// Base class for creating authorizers of requests.
/// </summary>
/// <remarks>
/// Derive from this class to configure authorization for a request type.  You
/// will implement the BuildPolicy method and declare requirements by calling
/// UseRequirement at least once.
/// </remarks>
/// <typeparam name="TRequest">The type of request being authorized</typeparam>
public abstract class AbstractRequestAuthorizer<TRequest> : IRequestAuthorizer<TRequest>
{
    private readonly HashSet<IRequestAuthorizationRequirement> _requirements = [];

    /// <inheritdoc/>
    public IEnumerable<IRequestAuthorizationRequirement> Requirements => _requirements;

    /// <inheritdoc/>
    protected void UseRequirement(IRequestAuthorizationRequirement requirement)
    {
        _requirements.Add(requirement);
    }

    /// <inheritdoc/>
    public abstract void BuildPolicy(TRequest request);
}
