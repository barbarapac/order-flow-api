using FluentAssertions;
using OrderFlow.Application.Orders.Place;

namespace OrderFlow.UnitTest.Application.Orders.Place;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        // Arrange
        var command = new PlaceOrderCommand(Guid.NewGuid(), "USD", [new PlaceOrderItemRequest(Guid.NewGuid(), 2)]);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDX")]
    public void Validate_WithInvalidCurrency_HasErrorForCurrency(string currency)
    {
        // Arrange
        var command = new PlaceOrderCommand(Guid.NewGuid(), currency, [new PlaceOrderItemRequest(Guid.NewGuid(), 1)]);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PlaceOrderCommand.Currency));
    }

    [Fact]
    public void Validate_WithNoItems_HasErrorForItems()
    {
        // Arrange
        var command = new PlaceOrderCommand(Guid.NewGuid(), "USD", []);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PlaceOrderCommand.Items));
    }

    [Fact]
    public void Validate_WithEmptyProductId_HasErrorForItemProductId()
    {
        // Arrange
        var command = new PlaceOrderCommand(Guid.NewGuid(), "USD", [new PlaceOrderItemRequest(Guid.Empty, 1)]);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items[0].ProductId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveQuantity_HasErrorForItemQuantity(int quantity)
    {
        // Arrange
        var command = new PlaceOrderCommand(Guid.NewGuid(), "USD", [new PlaceOrderItemRequest(Guid.NewGuid(), quantity)]);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items[0].Quantity");
    }
}
