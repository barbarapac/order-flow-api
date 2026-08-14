using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Orders.Create;

public sealed record CreteOrderCommand(Guid CustomerId, string Currency, IReadOnlyCollection<CreateOrderItemRequest> Items)
    : ICommand<Result<CreateOrderResponse>>;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);
