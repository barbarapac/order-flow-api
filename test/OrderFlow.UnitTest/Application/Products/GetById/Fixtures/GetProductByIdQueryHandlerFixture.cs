using OrderFlow.Application.Products.GetById;
using OrderFlow.UnitTest.Application.Products.GetById.Mocks;

namespace OrderFlow.UnitTest.Application.Products.GetById.Fixtures;

public class GetProductByIdQueryHandlerFixture
{
    protected QueryExecutorMock QueryExecutorMock { get; private set; }

    protected GetProductByIdQueryHandler Handler { get; private set; }

    protected GetProductByIdQueryHandlerFixture()
    {
        QueryExecutorMock = new QueryExecutorMock();

        Handler = new GetProductByIdQueryHandler(QueryExecutorMock.Object);
    }
}
