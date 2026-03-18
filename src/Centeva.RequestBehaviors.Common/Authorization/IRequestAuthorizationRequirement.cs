namespace Centeva.RequestBehaviors.Common.Authorization;

/// <summary>
/// Marker class for authorization requirements.
/// </summary>
/// <remarks>
/// Implementers of this interface will typically have some properties declared
/// to scope the request, such as the identifier of some entity or resource that
/// is being requested.
///
/// A companion <see cref="IRequestAuthorizationHandler{TRequirement}"/> will contain
/// the logic for determining whether a requirement is met.
/// </remarks>
/// <example>See docs/adr/0004-request-authorization.md</example>
public interface IRequestAuthorizationRequirement
{

}
