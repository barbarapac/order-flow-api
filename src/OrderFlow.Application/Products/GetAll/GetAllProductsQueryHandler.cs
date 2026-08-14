using Mediator;
using OrderFlow.Application._Shared;

namespace OrderFlow.Application.Products.GetAll;

public sealed class GetAllProductsQueryHandler(IQueryExecutor queryExecutor)
    : IQueryHandler<GetAllProductsQuery, PagedResult<GetAllProductsResponse>>
{
    public async ValueTask<PagedResult<GetAllProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var (totalCount, items) = await queryExecutor.QueryCountAndListAsync<GetAllProductsResponse>(
            Sql.CountAndList, 
            new { Skip = skip, request.PageSize }, 
            cancellationToken);

        return new PagedResult<GetAllProductsResponse>(items, request.Page, request.PageSize, totalCount);
    }
}
