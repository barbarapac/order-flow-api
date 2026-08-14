using Moq;
using OrderFlow.Domain.Products;

namespace OrderFlow.UnitTest.Application.Products.Create.Mocks;

public class ProductRepositoryMock : Mock<IProductRepository>
{
    public void VerifyAddWasCalledWith(string name, decimal unitPrice, int availableQuantity)
    {
        Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == name && p.UnitPrice == unitPrice && p.AvailableQuantity == availableQuantity)), Times.Once);
    }
}
