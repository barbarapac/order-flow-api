using Moq;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Products.DecrementStock.Mocks;

public class ProductRepositoryMock : Mock<IProductRepository>
{
    public void ConfigureDecrementStockToReturn(Guid productId, int quantity, int affectedRows)
    {
        Setup(r => r.DecrementStockAsync(productId, quantity, It.IsAny<CancellationToken>())).ReturnsAsync(affectedRows);
    }
}
