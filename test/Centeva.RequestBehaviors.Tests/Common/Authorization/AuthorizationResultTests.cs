using Centeva.RequestBehaviors.Common.Authorization;

namespace Centeva.RequestBehaviors.Tests.Common.Authorization;

public class AuthorizationResultTests
{
    [Fact]
    public void Fail_WithMessage_SetsFailureMessage()
    {
        var result = AuthorizationResult.Fail("test");
        
        result.IsAuthorized.Should().BeFalse();
        result.FailureMessage.Should().Be("test");
    }

    [Fact]
    public void Fail_WithoutMessage_SetsFailureMessageToNull()
    {
        var result = AuthorizationResult.Fail();

        result.IsAuthorized.Should().BeFalse();
        result.FailureMessage.Should().BeNull();
    }

    [Fact]
    public void Succeed_SetsIsAuthorizedToTrue()
    {
        var result = AuthorizationResult.Succeed();

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Succeed_SetsFailureMessageToNull()
    {
        var result = AuthorizationResult.Succeed();

        result.FailureMessage.Should().BeNull();
    }
}
