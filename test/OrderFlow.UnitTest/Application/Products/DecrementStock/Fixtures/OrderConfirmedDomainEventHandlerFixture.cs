using OrderFlow.Application.Products.OrderConfirmed;
using OrderFlow.UnitTest.Application.Products.DecrementStock.Mocks;

namespace OrderFlow.UnitTest.Application.Products.DecrementStock.Fixtures;

public class OrderConfirmedDomainEventHandlerFixture
{
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }
    protected DistributedLockMock DistributedLockMock { get; private set; }

    protected OrderConfirmedEventHandler Handler { get; private set; }

    protected OrderConfirmedDomainEventHandlerFixture()
    {
        ProductRepositoryMock = new ProductRepositoryMock();
        DistributedLockMock = new DistributedLockMock();

        Handler = new OrderConfirmedEventHandler(ProductRepositoryMock.Object, DistributedLockMock.Object);
    }
}
