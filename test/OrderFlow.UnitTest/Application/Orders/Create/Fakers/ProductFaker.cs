using AutoBogus;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Orders.Create.Fakers;

public static class ProductFaker
{
    public static Product Valid()
    {
        return new AutoFaker<Product>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.UnitPrice, f => f.Random.Decimal(0.01m, 1000m))
            .RuleFor(x => x.AvailableQuantity, f => f.Random.Int(10, 100))
            .Generate();
    }
}
