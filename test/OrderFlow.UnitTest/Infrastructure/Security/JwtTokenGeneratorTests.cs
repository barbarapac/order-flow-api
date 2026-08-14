using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using OrderFlow.UnitTest.Infrastructure.Security.Fakers;
using OrderFlow.UnitTest.Infrastructure.Security.Fixtures;

namespace OrderFlow.UnitTest.Infrastructure.Security;

public class JwtTokenGeneratorTests : JwtTokenGeneratorFixture
{
    [Fact]
    public void Generate_ProducesTokenThatExpiresAccordingToOptions()
    {
        // Arrange
        var user = UserFaker.Valid();

        // Act
        var (_, expiresAtUtc) = Generator.Generate(user);

        // Assert
        expiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(Settings.ExpirationMinutes), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Generate_ProducesTokenWithExpectedClaims()
    {
        // Arrange
        var user = UserFaker.Valid();

        // Act
        var (token, _) = Generator.Generate(user);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be(Settings.Issuer);
        jwt.Audiences.Should().Contain(Settings.Audience);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email.Value);
    }

    [Fact]
    public void Generate_TwoTokens_ProduceDifferentJtiClaims()
    {
        // Arrange
        var user = UserFaker.Valid();
        var handler = new JwtSecurityTokenHandler();

        // Act
        var (firstToken, _) = Generator.Generate(user);
        var (secondToken, _) = Generator.Generate(user);

        // Assert
        var firstJti = handler.ReadJwtToken(firstToken).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var secondJti = handler.ReadJwtToken(secondToken).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        firstJti.Should().NotBe(secondJti);
    }
}
