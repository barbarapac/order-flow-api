using FluentAssertions;
using OrderFlow.Application.Orders.Create;

namespace OrderFlow.UnitTest.Application.Orders.Create;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        // Arrange
        var command = new CreteOrderCommand(Guid.NewGuid(), "USD", [new CreateOrderItemRequest(Guid.NewGuid(), 2)]);

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
        var command = new CreteOrderCommand(Guid.NewGuid(), currency, [new CreateOrderItemRequest(Guid.NewGuid(), 1)]);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreteOrderCommand.Currency));
    }

    [Fact]
    public void Validate_WithNoItems_HasErrorForItems()
    {
        // Arrange
        var command = new CreteOrderCommand(Guid.NewGuid(), "USD", []);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreteOrderCommand.Items));
    }

    [Fact]
    public void Validate_WithEmptyProductId_HasErrorForItemProductId()
    {
        // Arrange
        var command = new CreteOrderCommand(Guid.NewGuid(), "USD", [new CreateOrderItemRequest(Guid.Empty, 1)]);

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
        var command = new CreteOrderCommand(Guid.NewGuid(), "USD", [new CreateOrderItemRequest(Guid.NewGuid(), quantity)]);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items[0].Quantity");
    }
}
