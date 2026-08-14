using OrderFlow.Application.Products.GetAll;
using OrderFlow.UnitTest.Application.Products.GetAll.Mocks;

namespace OrderFlow.UnitTest.Application.Products.GetAll.Fixtures;

public class GetAllProductsQueryHandlerFixture
{
    protected QueryExecutorMock QueryExecutorMock { get; private set; }

    protected GetAllProductsQueryHandler Handler { get; private set; }

    protected GetAllProductsQueryHandlerFixture()
    {
        QueryExecutorMock = new QueryExecutorMock();

        Handler = new GetAllProductsQueryHandler(QueryExecutorMock.Object);
    }
}
