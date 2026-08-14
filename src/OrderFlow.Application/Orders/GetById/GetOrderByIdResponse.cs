using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.GetById;

public sealed record GetOrderByIdResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    string Status,
    decimal Total,
    DateTime CreatedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? CanceledAtUtc,
    IReadOnlyCollection<GetOrderByIdItemResponse> Items)
{
    public static GetOrderByIdResponse From(Order order)
    {
        return new GetOrderByIdResponse(
            order.Id,
            order.CustomerId,
            order.Currency.Value,
            order.Status.ToString(),
            order.Total,
            order.CreatedAtUtc,
            order.ConfirmedAtUtc,
            order.CanceledAtUtc,
            order.Items.Select(GetOrderByIdItemResponse.From).ToList());
    }
}

public sealed record GetOrderByIdItemResponse(Guid ProductId, decimal UnitPrice, int Quantity, decimal LineTotal)
{
    public static GetOrderByIdItemResponse From(OrderItem item)
    {
        return new GetOrderByIdItemResponse(item.ProductId, item.UnitPrice, item.Quantity, item.LineTotal);
    }
}
