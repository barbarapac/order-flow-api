using OrderFlow.Application.Products.Update;
using OrderFlow.UnitTest.Application.Products.Update.Mocks;

namespace OrderFlow.UnitTest.Application.Products.Update.Fixtures;

public class UpdateProductCommandHandlerFixture
{
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }

    protected UpdateProductCommandHandler Handler { get; private set; }

    protected UpdateProductCommandHandlerFixture()
    {
        ProductRepositoryMock = new ProductRepositoryMock();
        UnitOfWorkMock = new UnitOfWorkMock();

        Handler = new UpdateProductCommandHandler(ProductRepositoryMock.Object, UnitOfWorkMock.Object);
    }
}
