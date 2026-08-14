using FluentAssertions;
using OrderFlow.Domain.Orders.Events;
using OrderFlow.UnitTest.Application.Products.OrderCanceled.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.OrderCanceled;

public class OrderCanceledEventHandlerTests : OrderCanceledEventHandlerFixture
{
    [Fact]
    public async Task Handle_WithMultipleItems_IncrementsEachItemAndReleasesLocksInOrder()
    {
        // Arrange
        var firstProductId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var secondProductId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var notification = new OrderFlow.Domain.Orders.Events.OrderCanceled(Guid.NewGuid(),
        [
            new OrderStockAdjustment(secondProductId, 3),
            new OrderStockAdjustment(firstProductId, 2)
        ]);

        ProductRepositoryMock.ConfigureIncrementStockToReturn(firstProductId, 2, 1);
        ProductRepositoryMock.ConfigureIncrementStockToReturn(secondProductId, 3, 1);

        // Act
        var act = () => Handler.Handle(notification, default).AsTask();

        // Assert
        await act.Should().NotThrowAsync();

        DistributedLockMock.AcquiredResources.Should().Equal(
            $"product:{firstProductId}:stock", $"product:{secondProductId}:stock");
        DistributedLockMock.ReleasedResources.Should().BeEquivalentTo(DistributedLockMock.AcquiredResources);
    }
}
