using OrderFlow.Application.Orders.GetById;
using OrderFlow.UnitTest.Application.Orders.GetById.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.GetById.Fixtures;

public class GetOrderByIdQueryHandlerFixture
{
    protected OrderRepositoryMock OrderRepositoryMock { get; private set; }

    protected GetOrderByIdQueryHandler Handler { get; private set; }

    protected GetOrderByIdQueryHandlerFixture()
    {
        OrderRepositoryMock = new OrderRepositoryMock();

        Handler = new GetOrderByIdQueryHandler(OrderRepositoryMock.Object);
    }
}
