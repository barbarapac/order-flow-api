using FluentAssertions;
using Mediator;
using Moq;
using OrderFlow.Application.Orders.Confirm;
using OrderFlow.Application.Products.OrderConfirmed;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders.Exceptions;
using OrderFlow.UnitTest.Application.Orders.Confirm.Fakers;
using OrderFlow.UnitTest.Application.Orders.Confirm.Fixtures;

namespace OrderFlow.UnitTest.Application.Orders.Confirm;

public class ConfirmOrderCommandHandlerTests : ConfirmOrderCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithPlacedOrder_ConfirmsPublishesEventAndPersists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Create(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);

        var command = new ConfirmOrderCommand(order.Id, customerId);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.Status.Should().Be("Confirmed");
        result.Value.ConfirmedAtUtc.Should().NotBeNull();

        PublisherMock.VerifyPublishWasCalledOnce();
        UnitOfWorkMock.VerifyTransactionWasCommitted();

        DistributedLockMock.AcquiredResources.Should().Equal($"order:{order.Id}:status");
        DistributedLockMock.ReleasedResources.Should().Equal($"order:{order.Id}:status");
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(orderId, customerId, null);

        var command = new ConfirmOrderCommand(orderId, customerId);

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
    public async Task Handle_WithAlreadyConfirmedOrder_IsIdempotentAndSkipsTransaction()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Confirmed(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);

        var command = new ConfirmOrderCommand(order.Id, customerId);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Confirmed");

        PublisherMock.VerifyPublishWasNeverCalled();
        UnitOfWorkMock.VerifyNoTransactionWasOpened();
    }

    [Fact]
    public async Task Handle_WhenPublishThrowsInsufficientStock_RollsBackTransactionAndPropagates()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Create(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);
        PublisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask(Task.FromException(InsufficientStockException.For(Guid.NewGuid()))));

        var command = new ConfirmOrderCommand(order.Id, customerId);

        // Act
        var act = () => Handler.Handle(command, default).AsTask();

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>();
        UnitOfWorkMock.VerifyTransactionWasRolledBack();

        DistributedLockMock.ReleasedResources.Should().Equal($"order:{order.Id}:status");
    }

    [Fact]
    public async Task Handle_WithCanceledOrder_ThrowsInvalidTransitionAndRollsBack()
    {
        // Arrange: simula a corrida entre Cancel e Confirm concorrentes no mesmo pedido —
        // o Cancel venceu o lock primeiro, e este Confirm chega depois com o pedido já terminal.
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Canceled(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);

        var command = new ConfirmOrderCommand(order.Id, customerId);

        // Act
        var act = () => Handler.Handle(command, default).AsTask();

        // Assert
        var exception = await act.Should().ThrowAsync<OrderException>();
        exception.Which.Code.Should().Be("order.invalid_transition");

        UnitOfWorkMock.VerifyTransactionWasRolledBack();
        DistributedLockMock.ReleasedResources.Should().Equal($"order:{order.Id}:status");
    }

    [Fact]
    public async Task Handle_WhenRequestIsAlreadyCanceledAndPublishThrows_StillRollsBackTransaction()
    {
        // Arrange: regressão para o bug em que o rollback usava o mesmo cancellationToken da
        // requisição — se ele já chegasse cancelado (cliente desconectou), RollbackTransactionAsync
        // lançava OperationCanceledException antes de tocar o banco, mascarando a exception
        // original e deixando a transação aberta. O rollback deve sempre rodar com um token não
        // cancelado.
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Create(customerId);
        OrderRepositoryMock.ConfigureGetTrackedByIdToReturn(order.Id, customerId, order);
        PublisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask(Task.FromException(InsufficientStockException.For(Guid.NewGuid()))));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new ConfirmOrderCommand(order.Id, customerId);

        // Act
        var act = () => Handler.Handle(command, cts.Token).AsTask();

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>();
        UnitOfWorkMock.Verify(
            u => u.RollbackTransactionAsync(It.Is<CancellationToken>(t => !t.IsCancellationRequested)),
            Times.Once);
    }
}
