using FluentAssertions;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Orders.Enums;
using OrderFlow.Domain.Orders.Events;
using OrderFlow.Domain.Orders.Exceptions;

namespace OrderFlow.UnitTest.Domain.Orders;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_CreatesOrderAsPlaced()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<NewOrderItem>
        {
            new(Guid.NewGuid(), 10m, 2),
            new(Guid.NewGuid(), 5m, 3)
        };

        // Act
        var order = Order.Create(customerId, "USD", items);

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
    public void Create_WithoutItems_ThrowsDomainException()
    {
        // Arrange
        var act = () => Order.Create(Guid.NewGuid(), "USD", []);

        // Act & Assert
        act.Should().Throw<OrderException>()
            .Which.Code.Should().Be("order.no_items");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidQuantity_ThrowsDomainException(int invalidQuantity)
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, invalidQuantity) };
        var act = () => Order.Create(Guid.NewGuid(), "USD", items);

        // Act & Assert
        act.Should().Throw<OrderException>()
            .Which.Code.Should().Be("order.invalid_quantity");
    }

    [Fact]
    public void Create_WithInvalidCurrency_ThrowsDomainException()
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 1) };
        var act = () => Order.Create(Guid.NewGuid(), "US", items);

        // Act & Assert
        act.Should().Throw<OrderException>()
            .Which.Code.Should().Be("order.invalid_currency");
    }

    [Fact]
    public void Create_TwoOrders_GetDifferentIds()
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 1) };

        // Act
        var first  = Order.Create(Guid.NewGuid(), "USD", items);
        var second = Order.Create(Guid.NewGuid(), "USD", items);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Confirm_WhenPlaced_TransitionsToConfirmedAndReturnsEvent()
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 2), new(Guid.NewGuid(), 5m, 3) };
        var order = Order.Create(Guid.NewGuid(), "USD", items);

        // Act
        var domainEvent = order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAtUtc.Should().NotBeNull();
        order.ConfirmedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.Items.Should().BeEquivalentTo(items.Select(i => new OrderStockAdjustment(i.ProductId, i.Quantity)));
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ThrowsDomainException()
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 1) };
        var order = Order.Create(Guid.NewGuid(), "USD", items);
        order.Confirm();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<OrderException>()
            .Which.Code.Should().Be("order.invalid_transition");
    }

    [Fact]
    public void Confirm_WhenCanceled_ThrowsDomainException()
    {
        // Arrange: simula a corrida entre Confirm e Cancel concorrentes no mesmo pedido —
        // o Cancel venceu, e o Confirm chega depois já com o pedido em estado terminal.
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 1) };
        var order = Order.Create(Guid.NewGuid(), "USD", items);
        order.Cancel();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<OrderException>()
            .Which.Code.Should().Be("order.invalid_transition");
    }

    [Fact]
    public void Cancel_WhenPlaced_TransitionsToCanceledAndReturnsNoEvent()
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 2) };
        var order = Order.Create(Guid.NewGuid(), "USD", items);

        // Act
        var domainEvent = order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Canceled);
        order.CanceledAtUtc.Should().NotBeNull();
        order.CanceledAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        domainEvent.Should().BeNull();
    }

    [Fact]
    public void Cancel_WhenConfirmed_TransitionsToCanceledAndReturnsEventToReleaseStock()
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 2), new(Guid.NewGuid(), 5m, 3) };
        var order = Order.Create(Guid.NewGuid(), "USD", items);
        order.Confirm();

        // Act
        var domainEvent = order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Canceled);
        order.CanceledAtUtc.Should().NotBeNull();

        domainEvent.Should().NotBeNull();
        domainEvent!.OrderId.Should().Be(order.Id);
        domainEvent.Items.Should().BeEquivalentTo(items.Select(i => new OrderStockAdjustment(i.ProductId, i.Quantity)));
    }

    [Fact]
    public void Cancel_WhenAlreadyCanceled_ThrowsDomainException()
    {
        // Arrange
        var items = new List<NewOrderItem> { new(Guid.NewGuid(), 10m, 1) };
        var order = Order.Create(Guid.NewGuid(), "USD", items);
        order.Cancel();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<OrderException>()
            .Which.Code.Should().Be("order.invalid_transition");
    }
}
