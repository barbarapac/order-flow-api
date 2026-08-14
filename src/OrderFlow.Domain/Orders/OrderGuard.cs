namespace OrderFlow.Domain.Orders;

internal static class OrderGuard
{
    public static void HasItems(IReadOnlyCollection<OrderItemDraft> items)
    {
        if (items.Count == 0)
            throw OrderDomainException.NoItems();
    }

    public static void QuantityIsPositive(int quantity)
    {
        if (quantity <= 0)
            throw OrderDomainException.InvalidQuantity(quantity);
    }
}
