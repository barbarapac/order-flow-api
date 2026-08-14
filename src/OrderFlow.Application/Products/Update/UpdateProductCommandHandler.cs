using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.Update;

public sealed class UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>>
{
    public async ValueTask<Result<UpdateProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result<UpdateProductResponse>.Failure(
                Error.NotFound("product.not_found", $"Produto '{request.Name}' não encontrado."));
        }

        product.Name              = request.Name;
        product.UnitPrice         = request.UnitPrice;
        product.AvailableQuantity = request.AvailableQuantity;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateProductResponse>.Success(UpdateProductResponse.From(product));
    }
}
