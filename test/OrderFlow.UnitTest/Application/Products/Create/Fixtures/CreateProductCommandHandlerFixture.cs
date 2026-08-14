using OrderFlow.Application.Products.Create;
using OrderFlow.UnitTest.Application.Products.Create.Mocks;

namespace OrderFlow.UnitTest.Application.Products.Create.Fixtures;

public class CreateProductCommandHandlerFixture
{
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }

    protected CreateProductCommandHandler Handler { get; private set; }

    protected CreateProductCommandHandlerFixture()
    {
        ProductRepositoryMock = new ProductRepositoryMock();
        UnitOfWorkMock = new UnitOfWorkMock();

        Handler = new CreateProductCommandHandler(ProductRepositoryMock.Object, UnitOfWorkMock.Object);
    }
}
