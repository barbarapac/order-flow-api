using FluentAssertions;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders.Exceptions;
using OrderFlow.Domain.Orders.ValueObjects;

namespace OrderFlow.UnitTest.Domain.Orders;

public class CurrencyTests
{
    [Theory]
    [InlineData("  usd  ", "USD")]
    [InlineData("brl", "BRL")]
    [InlineData("EUR", "EUR")]
    public void Create_TrimsAndUppercases_ValidCodes(string raw, string expected)
    {
        // Act
        var currency = Currency.Create(raw);

        // Assert
        currency.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDT")]
    [InlineData("U5D")]
    public void Create_Throws_ForInvalidCodes(string raw)
    {
        // Arrange
        var act = () => Currency.Create(raw);

        // Act & Assert
        act.Should().Throw<OrderException>()
            .Which.Code.Should().Be("order.invalid_currency");
    }

    [Fact]
    public void Create_Throws_WithValidationErrorType()
    {
        // Arrange
        var act = () => Currency.Create("XX");

        // Act & Assert
        act.Should().Throw<OrderException>()
            .Which.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void TwoCurrencies_WithSameValue_AreEqual()
    {
        // Arrange
        var first = Currency.Create("usd");

        // Act
        var second = Currency.Create("USD");

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void ToString_ReturnsTheNormalizedCode()
    {
        // Arrange
        var currency = Currency.Create(" brl ");

        // Act
        var text = currency.ToString();

        // Assert
        text.Should().Be("BRL");
    }
}
