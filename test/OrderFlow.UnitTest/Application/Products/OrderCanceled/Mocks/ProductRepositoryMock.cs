using Moq;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Products.OrderCanceled.Mocks;

public class ProductRepositoryMock : Mock<IProductRepository>
{
    public void ConfigureIncrementStockToReturn(Guid productId, int quantity, int affectedRows)
    {
        Setup(r => r.IncrementStockAsync(productId, quantity, It.IsAny<CancellationToken>())).ReturnsAsync(affectedRows);
    }
}
