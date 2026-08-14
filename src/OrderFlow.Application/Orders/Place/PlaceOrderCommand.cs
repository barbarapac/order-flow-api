using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Orders.Place;

public sealed record PlaceOrderCommand(Guid CustomerId, string Currency, IReadOnlyCollection<PlaceOrderItemRequest> Items)
    : ICommand<Result<PlaceOrderResponse>>;

public sealed record PlaceOrderItemRequest(Guid ProductId, int Quantity);
