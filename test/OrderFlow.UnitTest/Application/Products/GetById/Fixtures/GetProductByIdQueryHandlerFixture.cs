using OrderFlow.Application.Products.GetById;
using OrderFlow.UnitTest.Application.Products.GetById.Mocks;

namespace OrderFlow.UnitTest.Application.Products.GetById.Fixtures;

public class GetProductByIdQueryHandlerFixture
{
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }

    protected GetProductByIdQueryHandler Handler { get; private set; }

    protected GetProductByIdQueryHandlerFixture()
    {
        ProductRepositoryMock = new ProductRepositoryMock();

        Handler = new GetProductByIdQueryHandler(ProductRepositoryMock.Object);
    }
}
