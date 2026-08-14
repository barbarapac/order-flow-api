using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.GetAll;

public sealed record GetAllOrdersResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    string Status,
    decimal Total,
    DateTime CreatedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? CanceledAtUtc,
    IReadOnlyCollection<GetAllOrdersItemResponse> Items)
{
    public static GetAllOrdersResponse From(Order order)
    {
        return new GetAllOrdersResponse(
            order.Id,
            order.CustomerId,
            order.Currency.Value,
            order.Status.ToString(),
            order.Total,
            order.CreatedAtUtc,
            order.ConfirmedAtUtc,
            order.CanceledAtUtc,
            order.Items.Select(GetAllOrdersItemResponse.From).ToList());
    }
}

public sealed record GetAllOrdersItemResponse(Guid ProductId, decimal UnitPrice, int Quantity, decimal LineTotal)
{
    public static GetAllOrdersItemResponse From(OrderItem item)
    {
        return new GetAllOrdersItemResponse(item.ProductId, item.UnitPrice, item.Quantity, item.LineTotal);
    }
}
