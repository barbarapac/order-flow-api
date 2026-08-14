using AutoBogus;
using OrderFlow.Application.Products.Create;

namespace OrderFlow.UnitTest.Application.Products.Create.Fakers;

public static class CreateProductCommandFaker
{
    public static CreateProductCommand Valid()
    {
        return new AutoFaker<CreateProductCommand>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.UnitPrice, f => f.Random.Decimal(0.01m, 1000m))
            .RuleFor(x => x.AvailableQuantity, f => f.Random.Int(0, 100))
            .Generate();
    }
}
