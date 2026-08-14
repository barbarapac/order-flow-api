using OrderFlow.Application.Orders.Confirm;
using OrderFlow.UnitTest.Application.Orders.Confirm.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.Confirm.Fixtures;

public class ConfirmOrderCommandHandlerFixture
{
    protected OrderRepositoryMock OrderRepositoryMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }
    protected PublisherMock PublisherMock { get; private set; }

    protected ConfirmOrderCommandHandler Handler { get; private set; }

    protected ConfirmOrderCommandHandlerFixture()
    {
        OrderRepositoryMock = new OrderRepositoryMock();
        UnitOfWorkMock = new UnitOfWorkMock();
        PublisherMock = new PublisherMock();

        Handler = new ConfirmOrderCommandHandler(OrderRepositoryMock.Object, UnitOfWorkMock.Object, PublisherMock.Object);
    }
}
