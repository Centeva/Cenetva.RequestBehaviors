namespace Centeva.RequestBehaviors.Common.Authorization;

/// <summary>
/// Represents an authorization failure
/// </summary>
public class NotAuthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the NotAuthorizedException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotAuthorizedException(string message) : base(message) { }
}
