using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.GetById.Fakers;

public static class OrderFaker
{
    public static Order Valid(Guid? customerId = null)
    {
        return Order.Place(
            customerId ?? Guid.NewGuid(),
            "USD",
            [new OrderItemDraft(Guid.NewGuid(), 10m, 2)]);
    }
}
