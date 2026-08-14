using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.GetById;

public sealed record GetProductByIdResponse(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity, DateTime CreatedAtUtc)
{
    public static GetProductByIdResponse From(Product product)
    {
        return new GetProductByIdResponse(
            product.Id, 
            product.Name, 
            product.UnitPrice, 
            product.AvailableQuantity, 
            product.CreatedAtUtc);
    }
}
