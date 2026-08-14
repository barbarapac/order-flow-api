using OrderFlow.Application.Products.Delete;
using OrderFlow.UnitTest.Application.Products.Delete.Mocks;

namespace OrderFlow.UnitTest.Application.Products.Delete.Fixtures;

public class DeleteProductCommandHandlerFixture
{
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }

    protected DeleteProductCommandHandler Handler { get; private set; }

    protected DeleteProductCommandHandlerFixture()
    {
        ProductRepositoryMock = new ProductRepositoryMock();
        UnitOfWorkMock = new UnitOfWorkMock();

        Handler = new DeleteProductCommandHandler(ProductRepositoryMock.Object, UnitOfWorkMock.Object);
    }
}
