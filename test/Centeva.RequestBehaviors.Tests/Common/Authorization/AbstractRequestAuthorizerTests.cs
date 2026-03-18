using Centeva.RequestBehaviors.Common.Authorization;

namespace Centeva.RequestBehaviors.Tests.Common.Authorization;

public class AbstractRequestAuthorizerTests
{
    public class TestRequest
    {
        public int Id { get; set; }
    }

    public class MustHaveAccessRequirement : IRequestAuthorizationRequirement
    {
        public int Id { get; set; }
    }

    public class TestRequestAuthorizer : AbstractRequestAuthorizer<TestRequest>
    {
        public override void BuildPolicy(TestRequest request)
        {
            UseRequirement(new MustHaveAccessRequirement { Id = request.Id });
        }
    }

    [Fact]
    public void Constructor_InitializesWithEmptyRequirements()
    {
        var authorizer = new TestRequestAuthorizer();

        authorizer.Requirements.Should().BeEmpty();
    }

    [Fact]
    public void BuildPolicy_AddsRequirements()
    {
        var authorizer = new TestRequestAuthorizer();
        var request = new TestRequest { Id = 1 };

        authorizer.BuildPolicy(request);

        authorizer.Requirements.Count().Should().Be(1);
        var requirement = authorizer.Requirements.First();
        requirement.Should().BeOfType<MustHaveAccessRequirement>();
        ((MustHaveAccessRequirement)requirement).Id.Should().Be(1);
    }
}
