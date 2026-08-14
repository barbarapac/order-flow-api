using Moq;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.Place.Mocks;

public class OrderRepositoryMock : Mock<IOrderRepository>
{
    public void VerifyAddWasCalledWith(Guid customerId, string currency)
    {
        Verify(r => r.Add(It.Is<Order>(o => o.CustomerId == customerId && o.Currency.Value == currency)), Times.Once);
    }
}
