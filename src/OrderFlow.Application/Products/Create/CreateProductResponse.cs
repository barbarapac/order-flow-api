using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.Create;

public sealed record CreateProductResponse(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity, DateTime CreatedAtUtc)
{
    public static CreateProductResponse From(Product product)
    {
        return new CreateProductResponse(
            product.Id, 
            product.Name, 
            product.UnitPrice, 
            product.AvailableQuantity, 
            product.CreatedAtUtc);
    }
}
