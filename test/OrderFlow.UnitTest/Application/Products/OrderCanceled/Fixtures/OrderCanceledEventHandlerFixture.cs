using OrderFlow.Application.Products.OrderCanceled;
using OrderFlow.UnitTest.Application.Products.OrderCanceled.Mocks;

namespace OrderFlow.UnitTest.Application.Products.OrderCanceled.Fixtures;

public class OrderCanceledEventHandlerFixture
{
    protected ProductRepositoryMock ProductRepositoryMock { get; private set; }
    protected DistributedLockMock DistributedLockMock { get; private set; }

    protected OrderCanceledEventHandler Handler { get; private set; }

    protected OrderCanceledEventHandlerFixture()
    {
        ProductRepositoryMock = new ProductRepositoryMock();
        DistributedLockMock = new DistributedLockMock();

        Handler = new OrderCanceledEventHandler(ProductRepositoryMock.Object, DistributedLockMock.Object);
    }
}
