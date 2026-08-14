using OrderFlow.Application.Orders.GetAll;
using OrderFlow.UnitTest.Application.Orders.GetAll.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.GetAll.Fixtures;

public class GetAllOrdersQueryHandlerFixture
{
    protected OrderRepositoryMock OrderRepositoryMock { get; private set; }

    protected GetAllOrdersQueryHandler Handler { get; private set; }

    protected GetAllOrdersQueryHandlerFixture()
    {
        OrderRepositoryMock = new OrderRepositoryMock();

        Handler = new GetAllOrdersQueryHandler(OrderRepositoryMock.Object);
    }
}
