# Centeva Request Behaviors

Centeva.RequestBehaviors is a set of .NET libraries that provide commonly used
request pipeline behaviors for "Mediator"-style applications, supporting the
MediatR and Mediator.SourceGenerator libraries.  These behaviors can be used to
implement cross-cutting concerns such as validation and authorization in a
clean and reusable way that is not tied to any specific presentation layer
(e.g., ASP.Net Core).

## Table of Contents

- [Built With](#built-with)
- [Getting Started](#getting-started)
- [Validation](#validation)
  - [Using with MediatR](#using-with-mediatr)
  - [Using with Mediator.SourceGenerator](#using-with-mediatorsourcegenerator)
  - [Validating a Request](#validating-a-request)
    - [Validating with `Result` Types](#validating-with-result-types)
- [Authorization](#authorization)
  - [Using with MediatR](#using-with-mediatr-1)
  - [Using with Mediator.SourceGenerator](#using-with-mediatorsourcegenerator-1)
  - [Authorization Requirements](#authorization-requirements)
  - [Request Authorizers](#request-authorizers)
    - [Authorizing with `Result` Types](#authorizing-with-result-types)
- [Contributing](#contributing)
  - [Running Tests](#running-tests)
  - [Deployment](#deployment)
- [Resources](#resources)

## Built With

* [.NET 8](https://dot.net)
* [MediatR](https://mediatr.io/)
* [Mediator.SourceGenerator](https://github.com/martinothamar/Mediator)
* [FluentValidation](https://fluentvalidation.net/)
* [Ardalis.Result](https://github.com/ardalis/Result)

## Getting Started

Add a reference to `Centeva.RequestBehaviors.MediatR` or
`Centeva.RequestBehaviors.Mediator` in your project, depending on which mediator
library you have chosen.

If you are using multiple projects to separate Core/Domain,
Application, and Web API layers (i.e., "Clean" or "Ports and Adapters"
architecture) then reference from the project containing your request
handlers.

If you are migrating from the older `Centeva.RequestValidation` and
`Centeva.RequestAuthorization` libraries, see the [migration guide](docs/Migrating.md) for instructions on updating your code.

## Validation

NOTE: If you're also using the authorization behavior, you likely want to
register validation first, then add the Authorization behavior. This ensures
that validation happens first in your pipeline.

### Using with MediatR

Register the validators and add the MediatR pipeline behavior in your DI
container:

```csharp
builder.Services.AddMediatRRequestValidation(typeof(SampleValidator).Assembly);
```

### Using with Mediator.SourceGenerator

```csharp
builder.Services.AddMediatorRequestValidation(typeof(SampleValidator).Assembly);
```

### Validating a Request

To validate a request, add a validator class to your project that inherits from
`AbstractValidator<TRequest>`, where `TRequest` is the type of request you want
to validate.  For example:

```csharp
public class SampleValidator : AbstractValidator<SampleRequest>
{
    public SampleValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
```

If validation fails, a `ValidationException` will be thrown.  Your application
is responsible for handling those exceptions, possibly by mapping to a custom
HTTP response or a ProblemDetails object.

#### Validating with `Result` Types

If your request handler returns a `Result` or `Result<T>` type (from the
[Ardalis.Result](https://github.com/ardalis/Result) library), then validation
failures will be returned as an invalid `Result` containing the validation
errors instead of a thrown exception.
This allows you to handle validation errors in a more functional style, without
relying on exceptions for control flow.

```csharp
var result = await _mediator.Send(new SampleRequest());

if (!result.IsInvalid)
{
    // Do something here with result.ValidationErrors
}

// Respond normally here
```

## Authorization

### Using with MediatR

Register the MediatR pipeline behavior, authorizers, and handlers in your DI
container:

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddMediatRAuthorization(typeof(DeleteItemAuthorizer).Assembly);
```

OR if you have multiple assemblies with authorizers/handlers:

```csharp
...
builder.Services.AddMediatRAuthorization(typeof(DeleteItemAuthorizer).Assembly, typeof(OtherAuthorizer).Assembly);
...
```

You can alternately register your authorizers separately:

```csharp
builder.Services.AddRequestAuthorizersFromAssembly(typeof(DeleteItemAuthorizer).Assembly);
builder.Services.AddRequestAuthorizationHandlersFromAssembly(typeof(MustBeItemOwnerHandler).Assembly);
```

### Using with Mediator.SourceGenerator

Register the Mediator pipeline behavior, authorizers, and handlers in your DI
container:

```csharp
builder.Services.AddMediator(cfg =>
{
    ...
});
builder.Services.AddMediatorAuthorization(typeof(DeleteItemAuthorizer).Assembly);
```

OR if you have multiple assemblies with authorizers/handlers:

```csharp
...
builder.Services.AddMediatorAuthorization(typeof(DeleteItemAuthorizer).Assembly, typeof(OtherAuthorizer).Assembly);
...
```

You can alternately register your authorizers separately:

```csharp
builder.Services.AddRequestAuthorizersFromAssembly(typeof(DeleteItemAuthorizer).Assembly);
builder.Services.AddRequestAuthorizationHandlersFromAssembly(typeof(MustBeItemOwnerHandler).Assembly);
```

### Authorization Requirements

Implement `IRequestAuthorizationRequirement` and `IRequestAuthorizationHandler<TRequirement>`
to create reusable authorization rules:

```csharp
public class MustBeItemOwnerRequirement : IRequestAuthorizationRequirement
{
  public int UserId { get; set; }
  public int ItemId { get; set; }
}

public class MustBeItemOwnerHandler : IRequestAuthorizationHandler<MustBeItemOwnerRequirement>
{
  public MustBeItemOwnerHandler(IItemRepository itemRepository)
  {
    _itemRepository = itemRepository;
  }

  public async Task<AuthorizationResult> Handle(
    MustBeItemOwnerRequirement requirement,
    CancellationToken cancellationToken)
  {
    var item = await _itemRepository.Get(requirement.ItemId, cancellationToken);

    if (item != null && item.OwnerId == requirement.UserId)
      return AuthorizationResult.Succeed();

    return AuthorizationResult.Fail("Must be the owner of this item");
  }
}
```

Each `IAuthorizationHandler` must return either a success or failure result
based on the logic in the `Handle` method.

### Request Authorizers

Derive from `AbstractRequestAuthorizer<TRequest>` and implement `BuildPolicy` to
compose requirements for authorizing the given MediatR request:

```csharp
public class DeleteItemAuthorizer : AbstractRequestAuthorizer<DeleteItemCommand>
{
  public DeleteItemAuthorizer(ICurrentUserService currentUserService)
  {
      _currentUserService = currentUserService;
  }

  public override void BuildPolicy(DeleteItemCommand request)
  {
      UseRequirement(new MustBeItemOwnerRequirement
      {
        UserId = _currentUserService.UserId,
        ItemId = request.ItemId
      });
  }
}
```

#### Authorizing with `Result` Types

If your request handler returns a `Result` or `Result<T>` type (from the
[Ardalis.Result](https://github.com/ardalis/Result) library), then authorization
failures will be returned as an "Forbidden" `Result` containing the failure
message, if any, instead of a thrown exception.
This allows you to handle authorization in a more functional style, without
relying on exceptions for control flow.

```csharp
var result = await _mediator.Send(new SampleRequest());

if (result.IsForbidden)
{
    // Do something here like return an HTTP 403
}

// Respond normally here
```

## Contributing

Please use a Pull Request to suggest changes to this library.  As this is a
shared library, strict semantic versioning rules should be followed to avoid
unexpected breaking changes.

### Running Tests

From Windows, use the `dotnet test` command, or your Visual Studio Test
Explorer.

### Deployment

This library is released via GitHub Releases.

## Resources

Some of the code here is based heavily on this article: [Handling Authorization
In Clean Architecture with ASP.NET Core and
MediatR](https://levelup.gitconnected.com/handling-authorization-in-clean-architecture-with-asp-net-core-and-mediatr-6b91eeaa4d15)
from Austin Davies.
