using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Orders.GetById;

public sealed record GetOrderByIdQuery(Guid Id, Guid CustomerId) : IQuery<Result<GetOrderByIdResponse>>;
