using FluentAssertions;
using OrderFlow.Application.Users.Register;

namespace OrderFlow.UnitTest.Application.Users.Register;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        // Arrange
        var command = new RegisterUserCommand("Jane Doe", "jane@example.com", "S3cret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_HasErrorForName(string name)
    {
        // Arrange
        var command = new RegisterUserCommand(name, "jane@example.com", "S3cret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_HasErrorForEmail(string email)
    {
        // Arrange
        var command = new RegisterUserCommand("Jane Doe", email, "S3cret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_HasErrorForEmail()
    {
        // Arrange
        var command = new RegisterUserCommand("Jane Doe", "not-an-email", "S3cret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyPassword_HasErrorForPassword(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("Jane Doe", "jane@example.com", password);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Theory]
    [InlineData("Ab1")]
    [InlineData("Ab12345")]
    public void Validate_WithPasswordShorterThan8Characters_HasErrorForPassword(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("Jane Doe", "jane@example.com", password);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterUserCommand.Password) &&
            e.ErrorMessage == "Password must be at least 8 characters long.");
    }

    [Fact]
    public void Validate_WithPasswordWithoutLetter_HasErrorForPassword()
    {
        // Arrange
        var command = new RegisterUserCommand("Jane Doe", "jane@example.com", "12345678");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterUserCommand.Password) &&
            e.ErrorMessage == "Password must contain at least one letter.");
    }

    [Fact]
    public void Validate_WithPasswordWithoutDigit_HasErrorForPassword()
    {
        // Arrange
        var command = new RegisterUserCommand("Jane Doe", "jane@example.com", "abcdefgh");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterUserCommand.Password) &&
            e.ErrorMessage == "Password must contain at least one digit.");
    }
}
