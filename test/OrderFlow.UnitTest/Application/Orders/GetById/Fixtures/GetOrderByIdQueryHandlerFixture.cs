using OrderFlow.Application.Orders.GetById;
using OrderFlow.UnitTest.Application.Orders.GetById.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.GetById.Fixtures;

public class GetOrderByIdQueryHandlerFixture
{
    protected QueryExecutorMock QueryExecutorMock { get; private set; }

    protected GetOrderByIdQueryHandler Handler { get; private set; }

    protected GetOrderByIdQueryHandlerFixture()
    {
        QueryExecutorMock = new QueryExecutorMock();

        Handler = new GetOrderByIdQueryHandler(QueryExecutorMock.Object);
    }
}
