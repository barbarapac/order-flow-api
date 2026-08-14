using FluentAssertions;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Auth.Login.Fakers;
using OrderFlow.UnitTest.Application.Auth.Login.Fixtures;

namespace OrderFlow.UnitTest.Application.Auth.Login;

public class LoginCommandHandlerTests : LoginCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokenFromGenerator()
    {
        // Arrange
        var user = UserFaker.Valid();
        var command = LoginCommandFaker.For(user.Email.Value, "S3cret123");
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(60);

        UserRepositoryMock.ConfigureGetByEmailToReturn(user);
        PasswordHasherMock.ConfigureVerifyToReturn(true);
        JwtTokenGeneratorMock.ConfigureGenerateToReturn("signed-jwt", expiresAtUtc);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("signed-jwt");
        result.Value.ExpiresAtUtc.Should().Be(expiresAtUtc);
        result.Value.UserId.Should().Be(user.Id);
        UserRepositoryMock.VerifyGetByEmailWasCalledWith(user.Email.Value);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ReturnsUnauthorizedWithoutRevealingReason()
    {
        // Arrange
        var command = LoginCommandFaker.Valid();
        UserRepositoryMock.ConfigureGetByEmailToReturn(null);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        result.Error.Code.Should().Be("auth.invalid_credentials");
        JwtTokenGeneratorMock.VerifyGenerateWasNotCalled();
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = UserFaker.Valid();
        var command = LoginCommandFaker.For(user.Email.Value, "wrong-password");

        UserRepositoryMock.ConfigureGetByEmailToReturn(user);
        PasswordHasherMock.ConfigureVerifyToReturn(false);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("auth.invalid_credentials");
        JwtTokenGeneratorMock.VerifyGenerateWasNotCalled();
    }
}
