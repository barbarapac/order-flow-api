using AutoBogus;
using OrderFlow.Application.Products.GetById;

namespace OrderFlow.UnitTest.Application.Products.GetById.Fakers;

public static class ProductFaker
{
    public static GetProductByIdResponse Valid()
    {
        return new AutoFaker<GetProductByIdResponse>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.UnitPrice, f => f.Random.Decimal(0.01m, 1000m))
            .RuleFor(x => x.AvailableQuantity, f => f.Random.Int(0, 100))
            .Generate();
    }
}
