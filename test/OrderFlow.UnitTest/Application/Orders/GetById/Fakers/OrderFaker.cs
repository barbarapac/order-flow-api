using OrderFlow.Application.Orders.GetById;

namespace OrderFlow.UnitTest.Application.Orders.GetById.Fakers;

public static class OrderFaker
{
    public static GetOrderByIdResponse ValidHeader(Guid? customerId = null)
    {
        return new GetOrderByIdResponse
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId ?? Guid.NewGuid(),
            Currency = "USD",
            Status = "Placed",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static List<GetOrderByIdItemResponse> ValidItems()
    {
        return
        [
            new GetOrderByIdItemResponse { ProductId = Guid.NewGuid(), UnitPrice = 10m, Quantity = 2 }
        ];
    }
}
