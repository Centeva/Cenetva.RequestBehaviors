# Migration Guide

This guide will help you migrate from the deprecated
`Centeva.RequestAuthorization` and `Centeva.RequestValidation` packages to the
new `Centeva.RequestBehaviors` package family.

## Table of Contents

- [Overview](#overview)
- [Package Changes](#package-changes)
- [Breaking Changes](#breaking-changes)
- [Step-by-Step Migration](#step-by-step-migration)
  - [1. Update Package References](#1-update-package-references)
  - [2. Update Namespace Imports](#2-update-namespace-imports)
  - [3. Update Service Registration](#3-update-service-registration)
  - [4. Update Code References](#4-update-code-references)
- [What Stays the Same](#what-stays-the-same)
- [Validation Migration Details](#validation-migration-details)
- [Authorization Migration Details](#authorization-migration-details)

## Overview

The `Centeva.RequestBehaviors` package family consolidates and improves upon the
previous `Centeva.RequestAuthorization` and `Centeva.RequestValidation`
packages. The new structure provides:

- **Support for multiple mediator libraries** - Choose between MediatR or
  Mediator.SourceGenerator
- **Same core functionality** - All features from the old packages are preserved

## Package Changes

### Old Packages (Deprecated)

```xml
<PackageReference Include="Centeva.RequestAuthorization" Version="x.x.x" />
<PackageReference Include="Centeva.RequestValidation" Version="x.x.x" />
```

### New Packages

For **MediatR** users:

```xml
<PackageReference Include="Centeva.RequestBehaviors.MediatR" Version="1.0.0" />
```

For **Mediator.SourceGenerator** users:

```xml
<PackageReference Include="Centeva.RequestBehaviors.Mediator" Version="1.0.0" />
```

> **Note:** The `Centeva.RequestBehaviors.Common` package is automatically
> included as a dependency and does not need to be explicitly referenced.

## Breaking Changes

### Namespace Changes

| Old Namespace | New Namespace |
|---------------|---------------|
| `Centeva.RequestAuthorization` | `Centeva.RequestBehaviors.Common.Authorization` (abstractions)<br/>`Centeva.RequestBehaviors.MediatR.Authorization` (MediatR implementation)<br/>`Centeva.RequestBehaviors.Mediator.Authorization` (Mediator implementation) |
| `Centeva.RequestValidation` | `Centeva.RequestBehaviors.MediatR.FluentValidation` (MediatR implementation)<br/>`Centeva.RequestBehaviors.Mediator.FluentValidation` (Mediator implementation) |

### Service Registration Method Names

| Old Method | New Method (MediatR) | New Method (Mediator) |
|------------|---------------------|----------------------|
| `AddRequestValidation()` | `AddMediatRFluentValidation()` | `AddMediatorFluentValidation()` |
| `AddRequestAuthorization()` | `AddMediatRAuthorization()` | `AddMediatorAuthorization()` |

## Step-by-Step Migration

### 1. Update Package References

**Remove** the old packages from your `.csproj` file:

```xml
<!-- Remove these -->
<PackageReference Include="Centeva.RequestAuthorization" Version="x.x.x" />
<PackageReference Include="Centeva.RequestValidation" Version="x.x.x" />
```

**Add** the appropriate new package based on your mediator library:

For MediatR:

```xml
<PackageReference Include="Centeva.RequestBehaviors.MediatR" Version="1.0.0" />
```

For Mediator.SourceGenerator:

```xml
<PackageReference Include="Centeva.RequestBehaviors.Mediator" Version="1.0.0" />
```

### 2. Update Namespace Imports

Update your `using` statements throughout your codebase:

#### For Authorization Classes

**Old:**

```csharp
using Centeva.RequestAuthorization;
```

**New:**

```csharp
using Centeva.RequestBehaviors.Common.Authorization;
```

#### For Validation (Behavior Registration)

**Old:**

```csharp
using Centeva.RequestValidation;
```

**New (MediatR):**

```csharp
// No explicit using needed - extension methods are in Microsoft.Extensions.DependencyInjection namespace
```

**New (Mediator):**

```csharp
// No explicit using needed - extension methods are in Microsoft.Extensions.DependencyInjection namespace
```

### 3. Update Service Registration

Update your `Program.cs` or `Startup.cs` where you register the behaviors:

#### Validation Registration

**Old:**

```csharp
builder.Services.AddRequestValidation(typeof(MyValidator).Assembly);
```

**New (MediatR):**

```csharp
builder.Services.AddMediatRFluentValidation(typeof(MyValidator).Assembly);
```

**New (Mediator):**

```csharp
builder.Services.AddMediatorFluentValidation(typeof(MyValidator).Assembly);
```

#### Authorization Registration

**Old:**

```csharp
builder.Services.AddRequestAuthorization(typeof(MyAuthorizer).Assembly);
```

**New (MediatR):**

```csharp
builder.Services.AddMediatRAuthorization(typeof(MyAuthorizer).Assembly);
```

**New (Mediator):**

```csharp
builder.Services.AddMediatorAuthorization(typeof(MyAuthorizer).Assembly);
```

#### Multiple Assemblies

**Old:**

```csharp
builder.Services.AddRequestAuthorization(
    typeof(MyAuthorizer).Assembly,
    typeof(OtherAuthorizer).Assembly);
```

**New (MediatR):**

```csharp
builder.Services.AddMediatRAuthorization(
    typeof(MyAuthorizer).Assembly,
    typeof(OtherAuthorizer).Assembly);
```

**New (Mediator):**

```csharp
builder.Services.AddMediatorAuthorization(
    typeof(MyAuthorizer).Assembly,
    typeof(OtherAuthorizer).Assembly);
```

### 4. Update Code References

Most of your application code should not require changes. The core interfaces
and base classes maintain the same names and signatures:

- `IRequestAuthorizationRequirement` - No changes needed
- `IRequestAuthorizationHandler<TRequirement>` - No changes needed
- `AbstractRequestAuthorizer<TRequest>` - No changes needed
- `AuthorizationResult` - No changes needed
- `NotAuthorizedException` - No changes needed
- `IRequestAuthorizer<TRequest>` - No changes needed

Simply update the namespace imports as described in step 2.

## What Stays the Same

The following aspects of your code **do not need to change**:

### Authorization Handlers

```csharp
// This code works exactly the same in the new packages
public class MustBeItemOwnerHandler : IRequestAuthorizationHandler<MustBeItemOwnerRequirement>
{
    public async Task<AuthorizationResult> Handle(
        MustBeItemOwnerRequirement requirement,
        CancellationToken cancellationToken)
    {
        // Your authorization logic
        return AuthorizationResult.Succeed();
    }
}
```

### Request Authorizers

```csharp
// This code works exactly the same in the new packages
public class DeleteItemAuthorizer : AbstractRequestAuthorizer<DeleteItemCommand>
{
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

### FluentValidation Validators

```csharp
// This code works exactly the same in the new packages
public class MyRequestValidator : AbstractValidator<MyRequest>
{
    public MyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

### Result Type Handling

```csharp
// Validation with Result types - no changes needed
var result = await _mediator.Send(new MyRequest());
if (result.IsInvalid)
{
    // Handle validation errors
}

// Authorization with Result types - no changes needed
var result = await _mediator.Send(new MyRequest());
if (result.IsForbidden)
{
    // Handle authorization failure
}
```

## Validation Migration Details

### Key Differences

1. **Method Name Change**: `AddRequestValidation()` →
   `AddMediatRFluentValidation()` or `AddMediatorFluentValidation()`
2. **No Namespace Changes**: FluentValidation validators continue to use
   `FluentValidation` namespace
3. **Same Behavior**: Validation failures still throw `ValidationException` for
   non-Result types and return `Result.Invalid()` for Result types

### Example Migration

**Before:**

```csharp
using Centeva.RequestValidation;

// In Program.cs
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddRequestValidation(typeof(Program).Assembly);
```

**After:**

```csharp
// In Program.cs - no using statement needed
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddMediatRFluentValidation(typeof(Program).Assembly);
```

## Authorization Migration Details

### Key Differences

1. **Method Name Change**: `AddRequestAuthorization()` →
   `AddMediatRAuthorization()` or `AddMediatorAuthorization()`
2. **Namespace Change**: `Centeva.RequestAuthorization` →
   `Centeva.RequestBehaviors.Common.Authorization`
3. **Same Behavior**: Authorization failures still throw
   `NotAuthorizedException` for non-Result types and return `Result.Forbidden()`
   for Result types

### Example Migration

**Before:**

```csharp
using Centeva.RequestAuthorization;

// Authorization classes
public class MyRequirement : IRequestAuthorizationRequirement { }
public class MyHandler : IRequestAuthorizationHandler<MyRequirement> { }
public class MyAuthorizer : AbstractRequestAuthorizer<MyRequest> { }

// In Program.cs
builder.Services.AddRequestAuthorization(typeof(Program).Assembly);
```

**After:**

```csharp
using Centeva.RequestBehaviors.Common.Authorization;

// Authorization classes - same code, just different namespace
public class MyRequirement : IRequestAuthorizationRequirement { }
public class MyHandler : IRequestAuthorizationHandler<MyRequirement> { }
public class MyAuthorizer : AbstractRequestAuthorizer<MyRequest> { }

// In Program.cs
builder.Services.AddMediatRAuthorization(typeof(Program).Assembly);
```

## Complete Example

### Before (Old Packages)

**Program.cs:**

```csharp
using Centeva.RequestAuthorization;
using Centeva.RequestValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddRequestValidation(typeof(Program).Assembly);
builder.Services.AddRequestAuthorization(typeof(Program).Assembly);

var app = builder.Build();
// ... rest of app configuration
```

**MyAuthorizer.cs:**

```csharp
using Centeva.RequestAuthorization;

public class MyAuthorizer : AbstractRequestAuthorizer<MyRequest>
{
    public override void BuildPolicy(MyRequest request)
    {
        UseRequirement(new MyRequirement { Id = request.Id });
    }
}
```

### After (New Packages)

**Program.cs:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Registration order matters: validation should run before authorization
builder.Services.AddMediatRFluentValidation(typeof(Program).Assembly);
builder.Services.AddMediatRAuthorization(typeof(Program).Assembly);

var app = builder.Build();
// ... rest of app configuration
```

**MyAuthorizer.cs:**

```csharp
using Centeva.RequestBehaviors.Common.Authorization;

public class MyAuthorizer : AbstractRequestAuthorizer<MyRequest>
{
    public override void BuildPolicy(MyRequest request)
    {
        UseRequirement(new MyRequirement { Id = request.Id });
    }
}
```

## Troubleshooting

### Issue: "Type or namespace not found" errors

**Solution:** Ensure you've updated all namespace imports from
`Centeva.RequestAuthorization` to
`Centeva.RequestBehaviors.Common.Authorization` in files that use authorization
interfaces and classes.

### Issue: "Method not found" errors during registration

**Solution:** Make sure you're using the correct registration method name:

- For MediatR: `AddMediatRFluentValidation()` and `AddMediatRAuthorization()`
- For Mediator: `AddMediatorFluentValidation()` and `AddMediatorAuthorization()`

### Issue: Behaviors not executing

**Solution:** Verify that:

1. You've registered the behaviors in your DI container
2. You're using the correct package for your mediator library (MediatR vs
   Mediator.SourceGenerator)
3. Your registration order is correct (validation before authorization if using
   both)

### Issue: MediatR license concerns

**Note:** The `Centeva.RequestBehaviors.MediatR` package uses MediatR version
12.x, which is under the Apache 2.0 license. MediatR version 13+ requires a
commercial license. If you need to use MediatR 13+, you'll need to obtain the
appropriate license from the MediatR team.

## Migration Checklist

Use this checklist to ensure you've completed all migration steps:

- [ ] Remove old package references (`Centeva.RequestAuthorization`,
  `Centeva.RequestValidation`)
- [ ] Add new package reference (`Centeva.RequestBehaviors.MediatR` or
  `Centeva.RequestBehaviors.Mediator`)
- [ ] Update namespace imports in authorization-related files
- [ ] Update service registration method names in `Program.cs`/`Startup.cs`
- [ ] Build the solution to verify no compilation errors
- [ ] Run all tests to ensure functionality is preserved
- [ ] Review and update any documentation referencing the old packages
- [ ] Update any CI/CD pipelines that reference the old package names

## Need Help?

If you encounter issues not covered in this guide:

1. Review the main [README.md](../README.md) for usage examples
2. Check the test projects for implementation examples
3. Open an issue on GitHub with details about your migration scenario

## Additional Resources

- [Main README](../README.md) - Comprehensive usage guide
- [Validation Examples](../test/Centeva.RequestBehaviors.Tests/) - Test files
  showing validation scenarios
- [Authorization Examples](../test/Centeva.RequestBehaviors.Tests/) - Test files
  showing authorization scenarios
