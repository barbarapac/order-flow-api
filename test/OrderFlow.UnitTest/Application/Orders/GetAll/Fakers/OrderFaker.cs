using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.GetAll.Fakers;

public static class OrderFaker
{
    public static Order Valid(Guid? customerId = null)
    {
        return Order.Place(
            customerId ?? Guid.NewGuid(),
            "USD",
            [new OrderItemDraft(Guid.NewGuid(), 10m, 2)]);
    }

    public static List<Order> ManyValid(int count, Guid? customerId = null) =>
        Enumerable.Range(0, count).Select(_ => Valid(customerId)).ToList();
}
