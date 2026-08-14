using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId, Guid CustomerId) : ICommand<Result<CancelOrderResponse>>;
