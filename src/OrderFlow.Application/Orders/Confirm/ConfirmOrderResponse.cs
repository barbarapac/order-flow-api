using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Confirm;

public sealed record ConfirmOrderResponse(Guid Id, string Status, DateTime? ConfirmedAtUtc)
{
    public static ConfirmOrderResponse From(Order order) =>
        new(order.Id, order.Status.ToString(), order.ConfirmedAtUtc);
}
