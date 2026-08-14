using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.Delete;

public sealed class DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return Result.Failure(Error.NotFound("product.not_found", $"Produto '{request.Id}' não encontrado."));

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
