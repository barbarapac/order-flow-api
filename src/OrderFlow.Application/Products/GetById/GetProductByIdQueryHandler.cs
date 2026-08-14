using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Products.GetById;

public sealed class GetProductByIdQueryHandler(IQueryExecutor queryExecutor)
    : IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    public async ValueTask<Result<GetProductByIdResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await queryExecutor.QuerySingleOrDefaultAsync<GetProductByIdResponse>(
            Sql.GetById, 
            new { request.Id }, 
            cancellationToken);

        return product is null
            ? Result<GetProductByIdResponse>.Failure(Error.NotFound("product.not_found", $"Produto '{request.Id}' não encontrado."))
            : Result<GetProductByIdResponse>.Success(product);
    }
}
