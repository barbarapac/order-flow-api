using FluentAssertions;
using OrderFlow.Infrastructure.Auth;

namespace OrderFlow.UnitTest.Infrastructure.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hash = _sut.Hash("S3cret123");

        // Act
        var result = _sut.Verify("S3cret123", hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _sut.Hash("S3cret123");

        // Act
        var result = _sut.Verify("something-else", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_NeverReturnsThePlainPassword()
    {
        // Act
        var hash = _sut.Hash("S3cret123");

        // Assert
        hash.Should().NotBe("S3cret123");
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashesEachTime()
    {
        // Act
        var first = _sut.Hash("S3cret123");
        var second = _sut.Hash("S3cret123");

        // Assert
        first.Should().NotBe(second); // salted
    }
}
