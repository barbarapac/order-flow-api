using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.Create;

public sealed class CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name              = request.Name,
            UnitPrice         = request.UnitPrice,
            AvailableQuantity = request.AvailableQuantity
        };

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateProductResponse>.Success(CreateProductResponse.From(product));
    }
}
