using FluentAssertions;
using OrderFlow.Domain.Users;
using OrderFlow.Domain.Users.Exceptions;

namespace OrderFlow.UnitTest.Domain.Users;

public class UserTests
{
    [Fact]
    public void Register_WithValidData_CreatesUserAsCustomerByDefault()
    {
        // Arrange
        var name         = "Jane Doe";
        var email        = "jane@example.com";
        var passwordHash = "hashed-password";

        // Act
        var user = User.Register(name, email, passwordHash);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be(name);
        user.Email.Value.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithoutName_ThrowsDomainException(string invalidName)
    {
        // Arrange
        var act = () => User.Register(invalidName, "jane@example.com", "hashed-password");

        // Act & Assert
        act.Should().Throw<UserException>()
            .Which.Code.Should().Be("user.invalid_name");
    }

    [Fact]
    public void Register_WithInvalidEmail_ThrowsDomainException()
    {
        // Arrange
        var act = () => User.Register("Jane Doe", "not-an-email", "hashed-password");

        // Act & Assert
        act.Should().Throw<UserException>()
            .Which.Code.Should().Be("user.invalid_email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithoutPasswordHash_ThrowsDomainException(string invalidHash)
    {
        // Arrange
        var act = () => User.Register("Jane Doe", "jane@example.com", invalidHash);

        // Act & Assert
        act.Should().Throw<UserException>()
            .Which.Code.Should().Be("user.invalid_password_hash");
    }

    [Fact]
    public void Register_TwoUsers_GetDifferentIds()
    {
        // Arrange
        var first = User.Register("Jane Doe", "jane@example.com", "hash");

        // Act
        var second = User.Register("John Doe", "john@example.com", "hash");

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
