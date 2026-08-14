using FluentAssertions;
using OrderFlow.Application.Products.OrderConfirmed;
using OrderFlow.Domain.Orders;
using OrderFlow.UnitTest.Application.Products.DecrementStock.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.DecrementStock;

public class OrderConfirmedEventHandlerTests : OrderConfirmedDomainEventHandlerFixture
{
    [Fact]
    public async Task Handle_WithSufficientStockForAllItems_DecrementsEachItemAndReleasesLocksInOrder()
    {
        // Arrange
        var firstProductId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var secondProductId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var notification = new OrderConfirmedDomainEvent(Guid.NewGuid(),
        [
            new OrderStockAdjustment(secondProductId, 3),
            new OrderStockAdjustment(firstProductId, 2)
        ]);

        ProductRepositoryMock.ConfigureDecrementStockToReturn(firstProductId, 2, 1);
        ProductRepositoryMock.ConfigureDecrementStockToReturn(secondProductId, 3, 1);

        // Act
        var act = () => Handler.Handle(notification, default).AsTask();

        // Assert
        await act.Should().NotThrowAsync();

        DistributedLockMock.AcquiredResources.Should().Equal(
            $"product:{firstProductId}:stock", $"product:{secondProductId}:stock");
        DistributedLockMock.ReleasedResources.Should().BeEquivalentTo(DistributedLockMock.AcquiredResources);
    }

    [Fact]
    public async Task Handle_WhenAnyItemHasInsufficientStock_ThrowsInsufficientStockExceptionAndStillReleasesLocks()
    {
        // Arrange
        var sufficientProductId = Guid.NewGuid();
        var insufficientProductId = Guid.NewGuid();

        var notification = new OrderConfirmedDomainEvent(Guid.NewGuid(),
        [
            new OrderStockAdjustment(sufficientProductId, 1),
            new OrderStockAdjustment(insufficientProductId, 5)
        ]);

        ProductRepositoryMock.ConfigureDecrementStockToReturn(sufficientProductId, 1, 1);
        ProductRepositoryMock.ConfigureDecrementStockToReturn(insufficientProductId, 5, 0);

        // Act
        var act = () => Handler.Handle(notification, default).AsTask();

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>()
            .Where(e => e.Code == "order.insufficient_stock");

        DistributedLockMock.ReleasedResources.Should().BeEquivalentTo(DistributedLockMock.AcquiredResources);
        DistributedLockMock.AcquiredResources.Should().HaveCount(2);
    }
}
