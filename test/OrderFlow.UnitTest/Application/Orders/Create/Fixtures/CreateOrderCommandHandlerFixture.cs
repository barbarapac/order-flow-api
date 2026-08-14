using OrderFlow.Application.Orders.Create;
using OrderFlow.UnitTest.Application.Orders.Create.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.Create.Fixtures;

public class CreateOrderCommandHandlerFixture
{
    protected OrderRepositoryMock OrderRepositoryMock { get; private set; }
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }

    protected CreateOrderCommandHandler Handler { get; private set; }

    protected CreateOrderCommandHandlerFixture()
    {
        OrderRepositoryMock = new OrderRepositoryMock();
        ProductRepositoryMock = new ProductRepositoryMock();
        UnitOfWorkMock = new UnitOfWorkMock();

        Handler = new CreateOrderCommandHandler(OrderRepositoryMock.Object, ProductRepositoryMock.Object, UnitOfWorkMock.Object);
    }
}
