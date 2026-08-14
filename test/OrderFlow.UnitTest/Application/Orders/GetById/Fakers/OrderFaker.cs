using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.GetById.Fakers;

public static class OrderFaker
{
    public static Order Valid(Guid? customerId = null)
    {
        return Order.Create(
            customerId ?? Guid.NewGuid(),
            "USD",
            [new NewOrderItem(Guid.NewGuid(), 10m, 2)]);
    }
}
