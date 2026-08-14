using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.GetAll;

public sealed class GetAllProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetAllProductsQuery, PagedResult<GetAllProductsResponse>>
{
    public async ValueTask<PagedResult<GetAllProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await productRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        var items = products.Select(GetAllProductsResponse.From).ToList();

        return new PagedResult<GetAllProductsResponse>(items, request.Page, request.PageSize, totalCount);
    }
}
