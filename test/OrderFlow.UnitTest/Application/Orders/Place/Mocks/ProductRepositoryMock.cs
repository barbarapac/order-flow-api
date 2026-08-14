using Moq;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Orders.Place.Mocks;

public class ProductRepositoryMock : Mock<IProductRepository>
{
    public void ConfigureGetByIdToReturn(Guid id, Product? product)
    {
        Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
    }
}
