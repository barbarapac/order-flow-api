using OrderFlow.Application.Products.GetAll;
using OrderFlow.UnitTest.Application.Products.GetAll.Mocks;

namespace OrderFlow.UnitTest.Application.Products.GetAll.Fixtures;

public class GetAllProductsQueryHandlerFixture
{
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }

    protected GetAllProductsQueryHandler Handler { get; private set; }

    protected GetAllProductsQueryHandlerFixture()
    {
        ProductRepositoryMock = new ProductRepositoryMock();

        Handler = new GetAllProductsQueryHandler(ProductRepositoryMock.Object);
    }
}
