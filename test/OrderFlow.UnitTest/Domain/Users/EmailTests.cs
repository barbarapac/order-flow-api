using FluentAssertions;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Users;

namespace OrderFlow.UnitTest.Domain.Users;

public class EmailTests
{
    [Theory]
    [InlineData("  Jane.Doe@Example.COM  ", "jane.doe@example.com")]
    [InlineData("john@test.io", "john@test.io")]
    public void Create_TrimsAndLowercases_ValidAddresses(string raw, string expected)
    {
        // Act
        var email = Email.Create(raw);

        // Assert
        email.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    public void Create_Throws_ForInvalidAddresses(string raw)
    {
        // Arrange
        var act = () => Email.Create(raw);

        // Act & Assert
        act.Should().Throw<UserDomainException>()
            .Which.Code.Should().Be("user.invalid_email");
    }

    [Fact]
    public void Create_Throws_WithValidationErrorType()
    {
        // Arrange
        var act = () => Email.Create("invalid");

        // Act & Assert
        act.Should().Throw<UserDomainException>()
            .Which.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void TwoEmails_WithSameValue_AreEqual()
    {
        // Arrange
        var first = Email.Create("same@example.com");

        // Act
        var second = Email.Create("SAME@example.com");

        // Assert
        first.Should().Be(second);
    }
}
