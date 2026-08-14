using Moq;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Products.Delete.Mocks;

public class ProductRepositoryMock : Mock<IProductRepository>
{
    public void ConfigureGetByIdToReturn(Guid id, Product? product)
    {
        Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
    }

    public void VerifyRemoveWasCalledWith(Product product)
    {
        Verify(r => r.Remove(product), Times.Once);
    }

    public void VerifyRemoveWasNotCalled()
    {
        Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
    }
}
