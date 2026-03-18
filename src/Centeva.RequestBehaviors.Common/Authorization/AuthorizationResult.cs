namespace Centeva.RequestBehaviors.Common.Authorization;

/// <summary>
/// The result of an authorization check
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// True if request is authorized, false otherwise
    /// </summary>
    public bool IsAuthorized { get; }

    /// <summary>
    /// Description of failure reason if not authorized
    /// </summary>
    public string? FailureMessage { get; private set; }

    private AuthorizationResult(bool isAuthorized, string? failureMessage)
    {
        IsAuthorized = isAuthorized;
        FailureMessage = failureMessage;
    }

    /// <summary>
    /// Create a failure result without a message
    /// </summary>
    /// <returns></returns>
    public static AuthorizationResult Fail()
    {
        return new AuthorizationResult(false, null);
    }

    /// <summary>
    /// Create a failure result with a message
    /// </summary>
    /// <param name="failureMessage"></param>
    /// <returns></returns>
    public static AuthorizationResult Fail(string failureMessage)
    {
        return new AuthorizationResult(false, failureMessage);
    }

    /// <summary>
    /// Create a succeeded result
    /// </summary>
    /// <returns></returns>
    public static AuthorizationResult Succeed()
    {
        return new AuthorizationResult(true, null);
    }
}
