using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Confirm;

public sealed class ConfirmOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : ICommandHandler<ConfirmOrderCommand, Result<ConfirmOrderResponse>>
{
    public async ValueTask<Result<ConfirmOrderResponse>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetTrackedByIdAsync(request.OrderId, request.CustomerId, cancellationToken);

        if (order is null)
        {
            return Result<ConfirmOrderResponse>.Failure(Error.NotFound("order.not_found", $"Pedido '{request.OrderId}' não encontrado."));
        }

        if (order.IsConfirmed)
        {
            return Result<ConfirmOrderResponse>.Success(ConfirmOrderResponse.From(order));
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await publisher.Publish(order.Confirm(), cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result<ConfirmOrderResponse>.Success(ConfirmOrderResponse.From(order));
    }
}
