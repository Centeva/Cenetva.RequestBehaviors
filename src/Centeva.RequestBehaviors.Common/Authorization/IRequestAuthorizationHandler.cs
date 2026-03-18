namespace Centeva.RequestBehaviors.Common.Authorization;

/// <summary>
/// Defines an interface for classes that contain the logic for handling <see cref="IRequestAuthorizationRequirement"/> types
/// </summary>
/// <typeparam name="TRequirement"></typeparam>
public interface IRequestAuthorizationHandler<in TRequirement> where TRequirement : IRequestAuthorizationRequirement
{
    /// <summary>
    /// Calculate whether the given authorization requirement has been met
    /// </summary>
    /// <param name="requirement"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizationResult> Handle(TRequirement requirement, CancellationToken cancellationToken);
}
