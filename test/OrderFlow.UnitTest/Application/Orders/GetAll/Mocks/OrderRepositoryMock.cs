using Moq;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.GetAll.Mocks;

public class OrderRepositoryMock : Mock<IOrderRepository>
{
    public void ConfigureGetPagedToReturn(IReadOnlyCollection<Order> items, int totalCount)
    {
        Setup(r => r.GetPagedAsync(
                It.IsAny<Guid>(), It.IsAny<OrderStatus?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, totalCount));
    }
}
