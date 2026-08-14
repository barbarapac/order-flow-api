using OrderFlow.Domain.Orders.Enums;
using OrderFlow.Domain.Orders.Exceptions;

namespace OrderFlow.Domain.Orders;

internal static class OrderGuard
{
    public static void HasItems(IReadOnlyCollection<NewOrderItem> items)
    {
        if (items.Count == 0)
        {
            throw OrderException.NoItems();
        }
    }

    public static void QuantityIsPositive(int quantity)
    {
        if (quantity <= 0)
        {
            throw OrderException.InvalidQuantity(quantity);
        }
    }

    public static void CanConfirm(OrderStatus status)
    {
        if (status != OrderStatus.Placed)
        {
            throw OrderException.InvalidTransition(status, OrderStatus.Confirmed);
        }
    }

    public static void CanCancel(OrderStatus status)
    {
        if (status == OrderStatus.Canceled)
        {
            throw OrderException.InvalidTransition(status, OrderStatus.Canceled);
        }
    }
}
