using OrderFlow.Application.Orders.GetAll;
using OrderFlow.UnitTest.Application.Orders.GetAll.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.GetAll.Fixtures;

public class GetAllOrdersQueryHandlerFixture
{
    protected QueryExecutorMock QueryExecutorMock { get; private set; }

    protected GetAllOrdersQueryHandler Handler { get; private set; }

    protected GetAllOrdersQueryHandlerFixture()
    {
        QueryExecutorMock = new QueryExecutorMock();

        Handler = new GetAllOrdersQueryHandler(QueryExecutorMock.Object);
    }
}
