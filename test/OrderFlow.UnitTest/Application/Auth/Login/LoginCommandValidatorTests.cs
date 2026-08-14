using FluentAssertions;
using OrderFlow.Application.Auth.Login;

namespace OrderFlow.UnitTest.Application.Auth.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        // Arrange
        var command = new LoginCommand("jane@example.com", "S3cret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_HasErrorForEmail(string email)
    {
        // Arrange
        var command = new LoginCommand(email, "S3cret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_HasErrorForEmail()
    {
        // Arrange
        var command = new LoginCommand("not-an-email", "S3cret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyPassword_HasErrorForPassword(string password)
    {
        // Arrange
        var command = new LoginCommand("jane@example.com", password);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Validate_WithEmptyEmailAndPassword_HasErrorsForBoth()
    {
        // Arrange
        var command = new LoginCommand("", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
