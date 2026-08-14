using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.Confirm.Fakers;

public static class OrderFaker
{
    public static Order Create(Guid? customerId = null, IReadOnlyCollection<NewOrderItem>? items = null)
    {
        return Order.Create(
            customerId ?? Guid.NewGuid(),
            "USD",
            items ?? [new NewOrderItem(Guid.NewGuid(), 10m, 2)]);
    }

    public static Order Confirmed(Guid? customerId = null, IReadOnlyCollection<NewOrderItem>? items = null)
    {
        var order = Create(customerId, items);
        order.Confirm();
        return order;
    }
}
