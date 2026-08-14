using OrderFlow.Application.Orders.Place;
using OrderFlow.UnitTest.Application.Orders.Place.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.Place.Fixtures;

public class PlaceOrderCommandHandlerFixture
{
    protected OrderRepositoryMock OrderRepositoryMock { get; private set; }
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }

    protected PlaceOrderCommandHandler Handler { get; private set; }

    protected PlaceOrderCommandHandlerFixture()
    {
        OrderRepositoryMock = new OrderRepositoryMock();
        ProductRepositoryMock = new ProductRepositoryMock();
        UnitOfWorkMock = new UnitOfWorkMock();

        Handler = new PlaceOrderCommandHandler(OrderRepositoryMock.Object, ProductRepositoryMock.Object, UnitOfWorkMock.Object);
    }
}
