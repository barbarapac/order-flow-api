using Moq;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Products.GetAll.Mocks;

public class ProductRepositoryMock : Mock<IProductRepository>
{
    public void ConfigureGetPagedToReturn(IReadOnlyCollection<Product> products, int totalCount)
    {
        Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, totalCount));
    }
}
