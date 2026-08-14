using Moq;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.GetById.Mocks;

public class OrderRepositoryMock : Mock<IOrderRepository>
{
    public void ConfigureGetByIdToReturn(Guid id, Guid customerId, Order? order)
    {
        Setup(r => r.GetByIdAsync(id, customerId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
    }
}
