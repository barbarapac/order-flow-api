using Moq;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.Confirm.Mocks;

public class OrderRepositoryMock : Mock<IOrderRepository>
{
    public void ConfigureGetTrackedByIdToReturn(Guid id, Guid customerId, Order? order)
    {
        Setup(r => r.GetTrackedByIdAsync(id, customerId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
    }
}
