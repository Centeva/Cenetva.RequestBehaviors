namespace Centeva.RequestBehaviors.Common.Authorization;

/// <summary>
/// Defines an authorizer for a given request
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface IRequestAuthorizer<in TRequest>
{
    /// <summary>
    /// List of all requirements that must be met for successful authorization
    /// </summary>
    IEnumerable<IRequestAuthorizationRequirement> Requirements { get; }

    /// <summary>
    /// Builds an authorization policy (set of requirements) based on the incoming request
    /// </summary>
    /// <param name="instance"></param>
    void BuildPolicy(TRequest instance);
}
