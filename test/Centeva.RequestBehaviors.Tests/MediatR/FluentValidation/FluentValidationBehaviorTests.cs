using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Centeva.RequestBehaviors.Tests.MediatR.FluentValidation;

public class FluentValidationBehaviorTests
{
    private readonly ISender _mediator;

    public FluentValidationBehaviorTests()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<FluentValidationBehaviorTests>());
        services.AddMediatRFluentValidation(typeof(FluentValidationBehaviorTests).Assembly);

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<ISender>();
    }

    [Fact]
    public async Task Handle_WithNoValidators_DoesNotAffectResult()
    {
        var result = await _mediator.Send(new RequestWithoutValidation(), TestContext.Current.CancellationToken);

        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WithValidatorThatPasses_DoesNotAffectResult()
    {
        var result = await _mediator.Send(new ValidatedRequest { Value = "valid" }, TestContext.Current.CancellationToken);

        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WithValidatorThatPasses_WithGenericResult_ReturnsSuccessResult()
    {
        var result = await _mediator.Send(new RequestWithGenericResult { Value = "valid" }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WithValidatorThatPasses_WithNonGenericResult_ReturnsSuccessResult()
    {
        var result = await _mediator.Send(new RequestWithResult { Value = "valid" }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidatorThatFails_WithGenericResult_ReturnsInvalidResult()
    {
        var result = await _mediator.Send(new RequestWithGenericResult { Value = "" }, TestContext.Current.CancellationToken);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().HaveCount(1);

        var error = result.ValidationErrors.First();
        error.Identifier.Should().Be("Value");
        error.ErrorMessage.Should().Be("'Value' must not be empty.");
        error.Severity.Should().Be(ValidationSeverity.Error);
    }

    [Fact]
    public async Task Handle_WithValidatorThatFails_WithNonGenericResult_ReturnsInvalidResult()
    {
        var result = await _mediator.Send(new RequestWithResult { Value = "" }, TestContext.Current.CancellationToken);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().HaveCount(1);

        var error = result.ValidationErrors.First();
        error.Identifier.Should().Be("Value");
        error.ErrorMessage.Should().Be("'Value' must not be empty.");
        error.Severity.Should().Be(ValidationSeverity.Error);
    }

    [Fact]
    public async Task Handle_WithValidatorThatFails_ThrowsValidationException()
    {
        Func<Task> action = async () => await _mediator.Send(new ValidatedRequest { Value = "" }, TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_AggregatesAllFailures()
    {
        var result = await _mediator.Send(new RequestWithMultipleValidators { Value1 = "", Value2 = "" }, TestContext.Current.CancellationToken);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().HaveCount(2);

        var errors = result.ValidationErrors.ToList();
        errors.Should().Contain(e => e.Identifier == "Value1");
        errors.Should().Contain(e => e.Identifier == "Value2");
    }

    [Fact]
    public async Task Handle_WithMultipleFailuresInOneValidator_ReturnsAllErrors()
    {
        var result = await _mediator.Send(new RequestWithMultipleFailures { Value1 = "", Value2 = "" }, TestContext.Current.CancellationToken);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().HaveCount(2);

        var errors = result.ValidationErrors.ToList();
        errors.Should().Contain(e => e.Identifier == "Value1");
        errors.Should().Contain(e => e.Identifier == "Value2");
    }

    [Fact]
    public async Task Handle_WithCustomErrorCode_IncludesErrorCode()
    {
        var result = await _mediator.Send(new RequestWithErrorCode { Value = "" }, TestContext.Current.CancellationToken);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().HaveCount(1);

        var error = result.ValidationErrors.First();
        error.ErrorCode.Should().Be("CUSTOM_ERROR");
    }

    [Theory]
    [InlineData(Severity.Error, ValidationSeverity.Error)]
    [InlineData(Severity.Warning, ValidationSeverity.Warning)]
    [InlineData(Severity.Info, ValidationSeverity.Info)]
    public async Task Handle_WithSeverity_MapsCorrectly(Severity inputSeverity, ValidationSeverity expectedSeverity)
    {
        var request = new RequestWithConfigurableSeverity { Value = "", Severity = inputSeverity };
        var result = await _mediator.Send(request, TestContext.Current.CancellationToken);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().HaveCount(1);

        var error = result.ValidationErrors.First();
        error.Severity.Should().Be(expectedSeverity);
    }

    [Fact]
    public async Task Handle_WithCancellation_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> action = async () => await _mediator.Send(new RequestWithGenericResult { Value = "" }, cts.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    // Request and Handler classes
    public class RequestWithoutValidation : IRequest<string> { }

    public class RequestWithoutValidationHandler : IRequestHandler<RequestWithoutValidation, string>
    {
        public Task<string> Handle(RequestWithoutValidation request, CancellationToken cancellationToken)
        {
            return Task.FromResult("success");
        }
    }

    public class ValidatedRequest : IRequest<string>
    {
        public required string Value { get; set; }
    }

    public class ValidatedRequestValidator : AbstractValidator<ValidatedRequest>
    {
        public ValidatedRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    public class ValidatedRequestHandler : IRequestHandler<ValidatedRequest, string>
    {
        public Task<string> Handle(ValidatedRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult("success");
        }
    }

    public class RequestWithGenericResult : IRequest<Result<string>>
    {
        public required string Value { get; set; }
    }

    public class RequestWithGenericResultValidator : AbstractValidator<RequestWithGenericResult>
    {
        public RequestWithGenericResultValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    public class RequestWithGenericResultHandler : IRequestHandler<RequestWithGenericResult, Result<string>>
    {
        public Task<Result<string>> Handle(RequestWithGenericResult request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("success"));
        }
    }

    public class RequestWithResult : IRequest<Result>
    {
        public required string Value { get; set; }
    }

    public class RequestWithResultValidator : AbstractValidator<RequestWithResult>
    {
        public RequestWithResultValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    public class RequestWithResultHandler : IRequestHandler<RequestWithResult, Result>
    {
        public Task<Result> Handle(RequestWithResult request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    public class RequestWithMultipleValidators : IRequest<Result<string>>
    {
        public required string Value1 { get; set; }
        public required string Value2 { get; set; }
    }

    public class RequestWithMultipleValidatorsValidator1 : AbstractValidator<RequestWithMultipleValidators>
    {
        public RequestWithMultipleValidatorsValidator1()
        {
            RuleFor(x => x.Value1).NotEmpty();
        }
    }

    public class RequestWithMultipleValidatorsValidator2 : AbstractValidator<RequestWithMultipleValidators>
    {
        public RequestWithMultipleValidatorsValidator2()
        {
            RuleFor(x => x.Value2).NotEmpty();
        }
    }

    public class RequestWithMultipleValidatorsHandler : IRequestHandler<RequestWithMultipleValidators, Result<string>>
    {
        public Task<Result<string>> Handle(RequestWithMultipleValidators request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("success"));
        }
    }

    public class RequestWithMultipleFailures : IRequest<Result<string>>
    {
        public required string Value1 { get; set; }
        public required string Value2 { get; set; }
    }

    public class RequestWithMultipleFailuresValidator : AbstractValidator<RequestWithMultipleFailures>
    {
        public RequestWithMultipleFailuresValidator()
        {
            RuleFor(x => x.Value1).NotEmpty();
            RuleFor(x => x.Value2).NotEmpty();
        }
    }

    public class RequestWithMultipleFailuresHandler : IRequestHandler<RequestWithMultipleFailures, Result<string>>
    {
        public Task<Result<string>> Handle(RequestWithMultipleFailures request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("success"));
        }
    }

    public class RequestWithErrorCode : IRequest<Result<string>>
    {
        public required string Value { get; set; }
    }

    public class RequestWithErrorCodeValidator : AbstractValidator<RequestWithErrorCode>
    {
        public RequestWithErrorCodeValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithErrorCode("CUSTOM_ERROR");
        }
    }

    public class RequestWithErrorCodeHandler : IRequestHandler<RequestWithErrorCode, Result<string>>
    {
        public Task<Result<string>> Handle(RequestWithErrorCode request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("success"));
        }
    }

    public class RequestWithConfigurableSeverity : IRequest<Result<string>>
    {
        public required string Value { get; set; }
        public Severity Severity { get; set; }
    }

    public class RequestWithConfigurableSeverityValidator : AbstractValidator<RequestWithConfigurableSeverity>
    {
        public RequestWithConfigurableSeverityValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithSeverity(x => x.Severity);
        }
    }

    public class RequestWithConfigurableSeverityHandler : IRequestHandler<RequestWithConfigurableSeverity, Result<string>>
    {
        public Task<Result<string>> Handle(RequestWithConfigurableSeverity request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("success"));
        }
    }
}
