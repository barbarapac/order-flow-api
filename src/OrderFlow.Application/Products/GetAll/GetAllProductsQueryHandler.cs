using Mediator;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.GetAll;

public sealed class GetAllProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetAllProductsQuery, IReadOnlyCollection<GetAllProductsResponse>>
{
    public async ValueTask<IReadOnlyCollection<GetAllProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(GetAllProductsResponse.From).ToList();
    }
}
