using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Place;

public sealed record PlaceOrderResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    string Status,
    decimal Total,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<PlaceOrderItemResponse> Items)
{
    public static PlaceOrderResponse From(Order order)
    {
        return new PlaceOrderResponse(
            order.Id,
            order.CustomerId,
            order.Currency.Value,
            order.Status.ToString(),
            order.Total,
            order.CreatedAtUtc,
            order.Items.Select(PlaceOrderItemResponse.From).ToList());
    }
}

public sealed record PlaceOrderItemResponse(Guid ProductId, decimal UnitPrice, int Quantity, decimal LineTotal)
{
    public static PlaceOrderItemResponse From(OrderItem item)
    {
        return new PlaceOrderItemResponse(item.ProductId, item.UnitPrice, item.Quantity, item.LineTotal);
    }
}
