using Centeva.RequestBehaviors.Common.Authorization;

namespace Centeva.RequestBehaviors.Tests.Common.Authorization;

public class NotAuthorizedExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var exception = new NotAuthorizedException("test");

        exception.Message.Should().Be("test");
    }
}
