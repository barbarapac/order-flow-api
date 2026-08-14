using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.GetAll;

public sealed record GetAllProductsResponse(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity, DateTime CreatedAtUtc)
{
    public static GetAllProductsResponse From(Product product)
    {
        return new GetAllProductsResponse(
            product.Id, 
            product.Name,
            product.UnitPrice, 
            product.AvailableQuantity, 
            product.CreatedAtUtc);
    }
}
