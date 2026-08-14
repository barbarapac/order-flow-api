using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTest.Application.Orders.GetAll.Fakers;

public static class OrderFaker
{
    public static Order Valid(Guid? customerId = null)
    {
        return Order.Create(
            customerId ?? Guid.NewGuid(),
            "USD",
            [new NewOrderItem(Guid.NewGuid(), 10m, 2)]);
    }

    public static List<Order> ManyValid(int count, Guid? customerId = null) =>
        Enumerable.Range(0, count).Select(_ => Valid(customerId)).ToList();
}
