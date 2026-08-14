using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.GetAll;

public sealed class GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetAllOrdersQuery, PagedResult<GetAllOrdersResponse>>
{
    public async ValueTask<PagedResult<GetAllOrdersResponse>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await orderRepository.GetPagedAsync(
            request.CustomerId, request.Status, request.Page, request.PageSize, cancellationToken);

        var items = orders.Select(GetAllOrdersResponse.From).ToList();

        return new PagedResult<GetAllOrdersResponse>(items, request.Page, request.PageSize, totalCount);
    }
}
