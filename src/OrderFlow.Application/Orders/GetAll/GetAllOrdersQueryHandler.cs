using Mediator;
using OrderFlow.Application._Shared;

namespace OrderFlow.Application.Orders.GetAll;

public sealed class GetAllOrdersQueryHandler(IQueryExecutor queryExecutor)
    : IQueryHandler<GetAllOrdersQuery, PagedResult<GetAllOrdersResponse>>
{
    public async ValueTask<PagedResult<GetAllOrdersResponse>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var skip       = (request.Page - 1) * request.PageSize;
        var parameters = Sql.Parameters(request.CustomerId, request.Status, skip, request.PageSize);

        var (totalCount, headers) = await queryExecutor.QueryCountAndListAsync<GetAllOrdersResponse>(
            Sql.CountAndPage(request.Status), parameters, cancellationToken);

        if (headers.Count == 0)
        {
            return new PagedResult<GetAllOrdersResponse>([], request.Page, request.PageSize, totalCount);
        }

        var itemRows = await queryExecutor.QueryAsync<OrderItemRow>(
            Sql.ItemsByOrderIds, 
            new { OrderIds = headers.Select(h => h.Id).ToArray() },
            cancellationToken);

        var itemsByOrder = itemRows.ToLookup(r => r.OrderId);

        var items = headers.Select(h =>
        {
            var orderItems = itemsByOrder[h.Id]
                .Select(r => new GetAllOrdersItemResponse
                {
                    ProductId = r.ProductId,
                    UnitPrice = r.UnitPrice,
                    Quantity = r.Quantity,
                    LineTotal = r.UnitPrice * r.Quantity
                })
                .ToList();

            return h with { Total = orderItems.Sum(i => i.LineTotal), Items = orderItems };
        }).ToList();

        return new PagedResult<GetAllOrdersResponse>(items, request.Page, request.PageSize, totalCount);
    }
}
