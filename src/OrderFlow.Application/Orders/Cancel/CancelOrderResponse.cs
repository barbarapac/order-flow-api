using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Cancel;

public sealed record CancelOrderResponse(Guid Id, string Status, DateTime? CanceledAtUtc)
{
    public static CancelOrderResponse From(Order order) =>
        new(order.Id, order.Status.ToString(), order.CanceledAtUtc);
}
