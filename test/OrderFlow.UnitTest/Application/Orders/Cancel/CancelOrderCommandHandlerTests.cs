using FluentAssertions;
using Mediator;
using Moq;
using OrderFlow.Application.Orders.Cancel;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Orders.Cancel.Fakers;
using OrderFlow.UnitTest.Application.Orders.Cancel.Fixtures;

namespace OrderFlow.UnitTest.Application.Orders.Cancel;

public class CancelOrderCommandHandlerTests : CancelOrderCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithPlacedOrder_CancelsWithoutPublishingEventAndPersists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Create(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);

        var command = new CancelOrderCommand(order.Id, customerId);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.Status.Should().Be("Canceled");
        result.Value.CanceledAtUtc.Should().NotBeNull();

        PublisherMock.VerifyPublishWasNeverCalled();
        UnitOfWorkMock.VerifyTransactionWasCommitted();

        DistributedLockMock.AcquiredResources.Should().Equal($"order:{order.Id}:status");
        DistributedLockMock.ReleasedResources.Should().Equal($"order:{order.Id}:status");
    }

    [Fact]
    public async Task Handle_WithConfirmedOrder_CancelsPublishesStockReleaseEventAndPersists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Confirmed(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);

        var command = new CancelOrderCommand(order.Id, customerId);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Canceled");

        PublisherMock.VerifyPublishWasCalledOnce();
        UnitOfWorkMock.VerifyTransactionWasCommitted();
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(orderId, customerId, null);

        var command = new CancelOrderCommand(orderId, customerId);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("order.not_found");

        PublisherMock.VerifyPublishWasNeverCalled();
        UnitOfWorkMock.VerifyNoTransactionWasOpened();
    }

    [Fact]
    public async Task Handle_WithAlreadyCanceledOrder_IsIdempotentAndSkipsTransaction()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Canceled(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);

        var command = new CancelOrderCommand(order.Id, customerId);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Canceled");

        PublisherMock.VerifyPublishWasNeverCalled();
        UnitOfWorkMock.VerifyNoTransactionWasOpened();
    }

    [Fact]
    public async Task Handle_WhenPublishThrows_RollsBackTransactionAndPropagates()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Confirmed(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);
        PublisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask(Task.FromException(new InvalidOperationException("boom"))));

        var command = new CancelOrderCommand(order.Id, customerId);

        // Act
        var act = () => Handler.Handle(command, default).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        UnitOfWorkMock.VerifyTransactionWasRolledBack();

        DistributedLockMock.ReleasedResources.Should().Equal($"order:{order.Id}:status");
    }
}
