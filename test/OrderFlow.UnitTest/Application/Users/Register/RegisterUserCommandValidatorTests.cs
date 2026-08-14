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
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterUserCommand.Name) &&
            e.ErrorMessage == "O nome é obrigatório.");
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
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterUserCommand.Email) &&
            e.ErrorMessage == "O e-mail é obrigatório.");
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
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterUserCommand.Email) &&
            e.ErrorMessage == "E-mail em formato inválido.");
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
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterUserCommand.Password) &&
            e.ErrorMessage == "A senha é obrigatória.");
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
            e.ErrorMessage == "A senha deve ter no mínimo 8 caracteres.");
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
            e.ErrorMessage == "A senha deve conter pelo menos uma letra.");
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
            e.ErrorMessage == "A senha deve conter pelo menos um dígito.");
    }
}
