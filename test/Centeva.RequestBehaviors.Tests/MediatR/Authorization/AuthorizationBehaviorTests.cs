using Ardalis.Result;
using Centeva.RequestBehaviors.Common.Authorization;
using Centeva.RequestBehaviors.MediatR.Authorization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Centeva.RequestBehaviors.Tests.MediatR.Authorization;
public class AuthorizationBehaviorTests
{
    private readonly IServiceCollection _services;

    public AuthorizationBehaviorTests()
    {
        _services = new ServiceCollection();
        _services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
        });
    }

    [Fact]
    public async Task Handle_WithNoAuthorizers_DoesNotThrow()
    {
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorization { DesiredAuthResult = false };

        var response = await mediator.Send(request, TestContext.Current.CancellationToken);

        response.Should().Be("SUCCESS");
    }

    [Fact]
    public async Task Handle_WithNoMatchingAuthorizers_DoesNotThrow()
    {
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithoutAuthorization();

        var response = await mediator.Send(request, TestContext.Current.CancellationToken);

        response.Should().Be("SUCCESS");
    }

    [Fact]
    public async Task Handle_WithPassingAuthorizer_DoesNotThrow()
    {
        _services.AddRequestAuthorizersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        _services.AddRequestAuthorizationHandlersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorization { DesiredAuthResult = true };

        var response = await mediator.Send(request, TestContext.Current.CancellationToken);

        response.Should().Be("SUCCESS");
    }

    [Fact]
    public async Task Handle_WithFailingAuthorizer_ThrowsNotAuthorizedException()
    {
        _services.AddRequestAuthorizersFromAssemblies([typeof(AuthorizationBehaviorTests).Assembly]);
        _services.AddRequestAuthorizationHandlersFromAssemblies([typeof(AuthorizationBehaviorTests).Assembly]);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorization { DesiredAuthResult = false };

        var action = () => mediator.Send(request);

        var exception = await action.Should().ThrowAsync<NotAuthorizedException>()
            .WithMessage("Access denied");
    }

    [Fact]
    public async Task Handle_WhenAuthorizationHandlerMissing_Throws()
    {
        _services.AddRequestAuthorizersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorization { DesiredAuthResult = false };

        var action = () => mediator.Send(request);

        var exception = await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Could not find an authorization handler implementation for requirement type \"MustHaveAccessRequirement\"");
    }

    [Fact]
    public async Task Handle_WhenRequirementHasMultipleHandlers_Throws()
    {
        _services.AddRequestAuthorizersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        _services.AddRequestAuthorizationHandlersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithMultipleRequirementHandlers();

        var action = () => mediator.Send(request);

        var exception = await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Multiple authorization handler implementations were found for requirement type \"RequirementWithMultipleHandlers\"");
    }

    [Fact]
    public async Task Handle_WithFailingAuthorizer_ReturnsResultForbidden_WhenResponseIsResult()
    {
        _services.AddRequestAuthorizersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        _services.AddRequestAuthorizationHandlersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorizationReturningResult { DesiredAuthResult = false };

        var response = await mediator.Send(request, TestContext.Current.CancellationToken);

        response.Status.Should().Be(ResultStatus.Forbidden);
        response.Errors.Should().Contain("Access denied");
    }

    [Fact]
    public async Task Handle_WithPassingAuthorizer_ReturnsResultSuccess_WhenResponseIsResult()
    {
        _services.AddRequestAuthorizersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        _services.AddRequestAuthorizationHandlersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorizationReturningResult { DesiredAuthResult = true };

        var response = await mediator.Send(request, TestContext.Current.CancellationToken);

        response.Status.Should().Be(ResultStatus.Ok);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithFailingAuthorizer_ReturnsResultForbidden_WhenResponseIsGenericResult()
    {
        _services.AddRequestAuthorizersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        _services.AddRequestAuthorizationHandlersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorizationReturningGenericResult { DesiredAuthResult = false };

        var response = await mediator.Send(request, TestContext.Current.CancellationToken);

        response.Status.Should().Be(ResultStatus.Forbidden);
        response.Errors.Should().Contain("Access denied");
    }

    [Fact]
    public async Task Handle_WithPassingAuthorizer_ReturnsResultSuccess_WhenResponseIsGenericResult()
    {
        _services.AddRequestAuthorizersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        _services.AddRequestAuthorizationHandlersFromAssembly(typeof(AuthorizationBehaviorTests).Assembly);
        var provider = _services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ISender>();
        var request = new RequestWithAuthorizationReturningGenericResult { DesiredAuthResult = true };

        var response = await mediator.Send(request, TestContext.Current.CancellationToken);

        response.Status.Should().Be(ResultStatus.Ok);
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().Be("SUCCESS");
    }

    public class RequestWithAuthorization : IRequest<string>
    {
        public bool DesiredAuthResult { get; set; }
    }

    public class MustHaveAccessRequirement : IRequestAuthorizationRequirement
    {
        public bool HasAccess { get; set; }
    }

    public class MustHaveAccessRequirementHandler : IRequestAuthorizationHandler<MustHaveAccessRequirement>
    {
        public Task<AuthorizationResult> Handle(MustHaveAccessRequirement requirement, CancellationToken cancellationToken)
        {
            return requirement.HasAccess
                ? Task.FromResult(AuthorizationResult.Succeed())
                : Task.FromResult(AuthorizationResult.Fail("Access denied"));
        }
    }

    public class RequestWithAuthorizationAuthorizer : AbstractRequestAuthorizer<RequestWithAuthorization>
    {
        public override void BuildPolicy(RequestWithAuthorization request)
        {
            UseRequirement(new MustHaveAccessRequirement { HasAccess = request.DesiredAuthResult });
        }
    }

    public class RequestWithAuthorizationHandler : IRequestHandler<RequestWithAuthorization, string>
    {
        public Task<string> Handle(RequestWithAuthorization request, CancellationToken cancellationToken)
        {
            return Task.FromResult("SUCCESS");
        }
    }

    public class RequestWithoutAuthorization : IRequest<string>
    {
    }

    public class RequestWithoutAuthorizationHandler : IRequestHandler<RequestWithoutAuthorization, string>
    {
        public Task<string> Handle(RequestWithoutAuthorization request, CancellationToken cancellationToken)
        {
            return Task.FromResult("SUCCESS");
        }
    }

    public class RequestWithMultipleRequirementHandlers : IRequest<string>
    {
    }

    public class RequestWithMultipleRequirementHandlersHandler : IRequestHandler<RequestWithMultipleRequirementHandlers, string>
    {
        public Task<string> Handle(RequestWithMultipleRequirementHandlers request, CancellationToken cancellationToken)
        {
            return Task.FromResult("SUCCESS");
        }
    }

    public class RequestWithMultipleRequirementHandlersAuthorizer : AbstractRequestAuthorizer<RequestWithMultipleRequirementHandlers>
    {
        public override void BuildPolicy(RequestWithMultipleRequirementHandlers request)
        {
            UseRequirement(new RequirementWithMultipleHandlers());
        }
    }

    public class RequirementWithMultipleHandlers : IRequestAuthorizationRequirement
    {
    }

    public class RequirementWithMultipleHandlersHandler1 : IRequestAuthorizationHandler<RequirementWithMultipleHandlers>
    {
        public Task<AuthorizationResult> Handle(RequirementWithMultipleHandlers requirement, CancellationToken cancellationToken)
        {
            return Task.FromResult(AuthorizationResult.Succeed());
        }
    }

    public class RequirementWithMultipleHandlersHandler2 : IRequestAuthorizationHandler<RequirementWithMultipleHandlers>
    {
        public Task<AuthorizationResult> Handle(RequirementWithMultipleHandlers requirement, CancellationToken cancellationToken)
        {
            return Task.FromResult(AuthorizationResult.Succeed());
        }
    }

    public class RequestWithAuthorizationReturningResult : IRequest<Result>
    {
        public bool DesiredAuthResult { get; set; }
    }

    public class RequestWithAuthorizationReturningResultAuthorizer : AbstractRequestAuthorizer<RequestWithAuthorizationReturningResult>
    {
        public override void BuildPolicy(RequestWithAuthorizationReturningResult request)
        {
            UseRequirement(new MustHaveAccessRequirement { HasAccess = request.DesiredAuthResult });
        }
    }

    public class RequestWithAuthorizationReturningResultHandler : IRequestHandler<RequestWithAuthorizationReturningResult, Result>
    {
        public Task<Result> Handle(RequestWithAuthorizationReturningResult request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    public class RequestWithAuthorizationReturningGenericResult : IRequest<Result<string>>
    {
        public bool DesiredAuthResult { get; set; }
    }

    public class RequestWithAuthorizationReturningGenericResultAuthorizer : AbstractRequestAuthorizer<RequestWithAuthorizationReturningGenericResult>
    {
        public override void BuildPolicy(RequestWithAuthorizationReturningGenericResult request)
        {
            UseRequirement(new MustHaveAccessRequirement { HasAccess = request.DesiredAuthResult });
        }
    }

    public class RequestWithAuthorizationReturningGenericResultHandler : IRequestHandler<RequestWithAuthorizationReturningGenericResult, Result<string>>
    {
        public Task<Result<string>> Handle(RequestWithAuthorizationReturningGenericResult request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("SUCCESS"));
        }
    }
}
