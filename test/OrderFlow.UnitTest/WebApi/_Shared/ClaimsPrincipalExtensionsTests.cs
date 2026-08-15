using System.Security.Claims;
using FluentAssertions;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.UnitTest.WebApi._Shared;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ReadsTheIdFromTheNameIdentifierClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Bearer"));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_WhenTheClaimIsMissing_Throws()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var act = () => user.GetUserId();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetUserId_WhenTheClaimIsNotAGuid_Throws()
    {
        // Arrange
        var user = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "não-é-guid")], "Bearer"));

        // Act
        var act = () => user.GetUserId();

        // Assert
        act.Should().Throw<FormatException>();
    }
}
