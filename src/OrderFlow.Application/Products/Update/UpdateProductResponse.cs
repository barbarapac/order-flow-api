using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.Update;

public sealed record UpdateProductResponse(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity, DateTime CreatedAtUtc)
{
    public static UpdateProductResponse From(Product product)
    {
        return new UpdateProductResponse(
            product.Id, 
            product.Name, 
            product.UnitPrice, 
            product.AvailableQuantity, 
            product.CreatedAtUtc);
    }
}
