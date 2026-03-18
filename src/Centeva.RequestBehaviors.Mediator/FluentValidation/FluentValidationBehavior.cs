using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace Centeva.RequestBehaviors.Mediator.FluentValidation;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation validators.
/// For Result and Result&lt;T&gt; responses, validation failures are returned as Invalid results.
/// For other response types, throws ValidationException on failure.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class FluentValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IMessage
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the FluentValidationBehavior class with the specified validators.
    /// </summary>
    /// <param name="validators">The collection of validators to apply to the request. Cannot be null.</param>
    public FluentValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Processes the specified request by performing validation and invoking the next handler in the pipeline. If
    /// validation fails, returns an invalid result or throws a validation exception, depending on the response type.
    /// </summary>
    /// <param name="request">The request message to be validated and processed.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline to invoke if validation succeeds.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response from the next handler
    /// if validation succeeds, or an invalid result if validation fails and the response type supports it.</returns>
    /// <exception cref="ValidationException">Thrown if validation fails and the response type does not support invalid Results.</exception>
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(request, cancellationToken);
        }

        var validationResults =
            await Task.WhenAll(_validators.Select(v => v.ValidateAsync(request, cancellationToken)));
        var failures = validationResults
            .SelectMany(x => x.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(request, cancellationToken);
        }

        var validationErrors = CreateValidationErrors(failures);
        var responseType = typeof(TResponse);

        // Handle non-generic Result response type
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Invalid(validationErrors);
        }

        // Handle generic Result<T> response type
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var invalidMethod = responseType.GetMethod("Invalid", [typeof(List<ValidationError>)]);
            if (invalidMethod != null)
            {
                return (TResponse)invalidMethod.Invoke(null, [validationErrors])!;
            }
        }

        // Handle any other response types by throwing a ValidationException
        throw new ValidationException(failures);
    }

    private static List<ValidationError> CreateValidationErrors(List<ValidationFailure> failures)
    {
        return [.. failures
            .Select(f => new ValidationError(
                f.PropertyName,
                f.ErrorMessage,
                f.ErrorCode ?? string.Empty,
                FluentValidationResultExtensions.FromSeverity(f.Severity)))];
    }
}
