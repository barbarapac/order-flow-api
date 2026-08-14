using OrderFlow.Application.Orders.Cancel;
using OrderFlow.UnitTest.Application.Orders.Cancel.Mocks;

namespace OrderFlow.UnitTest.Application.Orders.Cancel.Fixtures;

public class CancelOrderCommandHandlerFixture
{
    protected OrderRepositoryMock OrderRepositoryMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }
    protected PublisherMock PublisherMock { get; private set; }
    protected DistributedLockMock DistributedLockMock { get; private set; }

    protected CancelOrderCommandHandler Handler { get; private set; }

    protected CancelOrderCommandHandlerFixture()
    {
        OrderRepositoryMock = new OrderRepositoryMock();
        UnitOfWorkMock = new UnitOfWorkMock();
        PublisherMock = new PublisherMock();
        DistributedLockMock = new DistributedLockMock();

        Handler = new CancelOrderCommandHandler(
            OrderRepositoryMock.Object, UnitOfWorkMock.Object, PublisherMock.Object, DistributedLockMock.Object);
    }
}
