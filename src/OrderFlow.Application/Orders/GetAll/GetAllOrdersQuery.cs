using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain.Orders.Enums;

namespace OrderFlow.Application.Orders.GetAll;

public sealed record GetAllOrdersQuery(Guid CustomerId, OrderStatus? Status, int Page, int PageSize)
    : IQuery<PagedResult<GetAllOrdersResponse>>;
