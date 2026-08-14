using Mediator;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.GetById;

public sealed class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    public async ValueTask<Result<GetProductByIdResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        return product is null
            ? Result<GetProductByIdResponse>.Failure(Error.NotFound("product.not_found", $"Produto '{request.Id}' não encontrado."))
            : Result<GetProductByIdResponse>.Success(GetProductByIdResponse.From(product));
    }
}
