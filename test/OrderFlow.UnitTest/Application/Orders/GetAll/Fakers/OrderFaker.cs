using OrderFlow.Application.Orders.GetAll;

namespace OrderFlow.UnitTest.Application.Orders.GetAll.Fakers;

public static class OrderFaker
{
    public static GetAllOrdersResponse ValidHeader(Guid? customerId = null)
    {
        return new GetAllOrdersResponse
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId ?? Guid.NewGuid(),
            Currency = "USD",
            Status = "Placed",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static List<GetAllOrdersResponse> ManyValidHeaders(int count, Guid? customerId = null) =>
        Enumerable.Range(0, count).Select(_ => ValidHeader(customerId)).ToList();

    public static OrderItemRow ItemRowFor(Guid orderId) =>
        new(orderId, Guid.NewGuid(), 10m, 2);
}
