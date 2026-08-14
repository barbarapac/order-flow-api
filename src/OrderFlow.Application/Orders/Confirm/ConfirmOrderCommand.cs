using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Orders.Confirm;

public sealed record ConfirmOrderCommand(Guid OrderId, Guid CustomerId) : ICommand<Result<ConfirmOrderResponse>>;
