using AutoBogus;
using OrderFlow.Application.Products.GetAll;

namespace OrderFlow.UnitTest.Application.Products.GetAll.Fakers;

public static class ProductFaker
{
    public static GetAllProductsResponse Valid()
    {
        return new AutoFaker<GetAllProductsResponse>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.UnitPrice, f => f.Random.Decimal(0.01m, 1000m))
            .RuleFor(x => x.AvailableQuantity, f => f.Random.Int(0, 100))
            .Generate();
    }

    public static List<GetAllProductsResponse> ManyValid(int count) =>
        Enumerable.Range(0, count).Select(_ => Valid()).ToList();
}
