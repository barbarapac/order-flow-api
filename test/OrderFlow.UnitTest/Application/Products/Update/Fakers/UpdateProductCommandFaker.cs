using AutoBogus;
using OrderFlow.Application.Products.Update;

namespace OrderFlow.UnitTest.Application.Products.Update.Fakers;

public static class UpdateProductCommandFaker
{
    public static UpdateProductCommand Valid()
    {
        return new AutoFaker<UpdateProductCommand>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.UnitPrice, f => f.Random.Decimal(0.01m, 1000m))
            .RuleFor(x => x.AvailableQuantity, f => f.Random.Int(0, 100))
            .Generate();
    }
}
