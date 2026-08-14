using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Create;

public sealed record CreateOrderResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    string Status,
    decimal Total,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<CreateOrderItemResponse> Items)
{
    public static CreateOrderResponse From(Order order)
    {
        return new CreateOrderResponse(
            order.Id,
            order.CustomerId,
            order.Currency.Value,
            order.Status.ToString(),
            order.Total,
            order.CreatedAtUtc,
            order.Items.Select(CreateOrderItemResponse.From).ToList());
    }
}

public sealed record CreateOrderItemResponse(Guid ProductId, decimal UnitPrice, int Quantity, decimal LineTotal)
{
    public static CreateOrderItemResponse From(OrderItem item)
    {
        return new CreateOrderItemResponse(item.ProductId, item.UnitPrice, item.Quantity, item.LineTotal);
    }
}
