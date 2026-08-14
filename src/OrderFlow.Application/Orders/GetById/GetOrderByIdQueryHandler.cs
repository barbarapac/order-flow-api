using Mediator;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.GetById;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
{
    public async ValueTask<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, request.CustomerId, cancellationToken);

        return order is null
            ? Result<GetOrderByIdResponse>.Failure(Error.NotFound("order.not_found", $"Pedido '{request.Id}' não encontrado."))
            : Result<GetOrderByIdResponse>.Success(GetOrderByIdResponse.From(order));
    }
}
