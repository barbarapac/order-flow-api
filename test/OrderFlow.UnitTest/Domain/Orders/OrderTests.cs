using FluentAssertions;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Domain.Orders;

public class OrderTests
{
    [Fact]
    public void Place_WithValidData_CreatesOrderAsPlaced()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<OrderItemDraft>
        {
            new(Guid.NewGuid(), 10m, 2),
            new(Guid.NewGuid(), 5m, 3)
        };

        // Act
        var order = Order.Place(customerId, "USD", items);

        // Assert
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(customerId);
        order.Currency.Value.Should().Be("USD");
        order.Status.Should().Be(OrderStatus.Placed);
        order.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.ConfirmedAtUtc.Should().BeNull();
        order.CanceledAtUtc.Should().BeNull();
        order.Items.Should().HaveCount(2);
        order.Total.Should().Be(35m);
    }

    [Fact]
    public void Place_WithoutItems_ThrowsDomainException()
    {
        // Arrange
        var act = () => Order.Place(Guid.NewGuid(), "USD", []);

        // Act & Assert
        act.Should().Throw<OrderDomainException>()
            .Which.Code.Should().Be("order.no_items");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Place_WithInvalidQuantity_ThrowsDomainException(int invalidQuantity)
    {
        // Arrange
        var items = new List<OrderItemDraft> { new(Guid.NewGuid(), 10m, invalidQuantity) };
        var act = () => Order.Place(Guid.NewGuid(), "USD", items);

        // Act & Assert
        act.Should().Throw<OrderDomainException>()
            .Which.Code.Should().Be("order.invalid_quantity");
    }

    [Fact]
    public void Place_WithInvalidCurrency_ThrowsDomainException()
    {
        // Arrange
        var items = new List<OrderItemDraft> { new(Guid.NewGuid(), 10m, 1) };
        var act = () => Order.Place(Guid.NewGuid(), "US", items);

        // Act & Assert
        act.Should().Throw<OrderDomainException>()
            .Which.Code.Should().Be("order.invalid_currency");
    }

    [Fact]
    public void Place_TwoOrders_GetDifferentIds()
    {
        // Arrange
        var items = new List<OrderItemDraft> { new(Guid.NewGuid(), 10m, 1) };

        // Act
        var first  = Order.Place(Guid.NewGuid(), "USD", items);
        var second = Order.Place(Guid.NewGuid(), "USD", items);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
