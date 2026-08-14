using Moq;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Products.GetAll.Mocks;

public class ProductRepositoryMock : Mock<IProductRepository>
{
    public void ConfigureGetAllToReturn(IReadOnlyCollection<Product> products)
    {
        Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);
    }
}
