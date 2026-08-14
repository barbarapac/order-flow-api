using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.Confirm.Fakers;

public static class OrderFaker
{
    public static Order Placed(Guid? customerId = null, IReadOnlyCollection<OrderItemDraft>? items = null)
    {
        return Order.Place(
            customerId ?? Guid.NewGuid(),
            "USD",
            items ?? [new OrderItemDraft(Guid.NewGuid(), 10m, 2)]);
    }

    public static Order Confirmed(Guid? customerId = null, IReadOnlyCollection<OrderItemDraft>? items = null)
    {
        var order = Placed(customerId, items);
        order.Confirm();
        return order;
    }
}
