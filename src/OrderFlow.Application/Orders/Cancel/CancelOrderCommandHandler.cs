using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Cancel;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IDistributedLock distributedLock)
    : ICommandHandler<CancelOrderCommand, Result<CancelOrderResponse>>
{
    public async ValueTask<Result<CancelOrderResponse>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        await using var orderLock = await distributedLock.AcquireAsync($"order:{request.OrderId}:status", cancellationToken);

        var order = await orderRepository.GetTrackedByIdAsync(request.OrderId, request.CustomerId, cancellationToken);

        if (order is null)
        {
            return Result<CancelOrderResponse>.Failure(Error.NotFound("order.not_found", $"Pedido '{request.OrderId}' não encontrado."));
        }

        if (order.IsCanceled)
        {
            return Result<CancelOrderResponse>.Success(CancelOrderResponse.From(order));
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var domainEvent = order.Cancel();

            if (domainEvent is not null)
            {
                await publisher.Publish(domainEvent, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }

        return Result<CancelOrderResponse>.Success(CancelOrderResponse.From(order));
    }
}
