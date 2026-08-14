namespace OrderFlow.Domain.Orders;

public sealed class Order
{
    public Guid Id                    { get; private set; }
    public Guid CustomerId            { get; private set; }
    public Currency Currency          { get; private set; }
    public OrderStatus Status         { get; private set; }
    public DateTime CreatedAtUtc      { get; private set; }
    public DateTime? ConfirmedAtUtc   { get; private set; }
    public DateTime? CanceledAtUtc    { get; private set; }
    public bool IsConfirmed => Status == OrderStatus.Confirmed;
    public bool IsCanceled  => Status == OrderStatus.Canceled;
    public decimal Total => _items.Sum(i => i.LineTotal);

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    private readonly List<OrderItem> _items = [];

    private Order(Guid customerId, Currency currency)
    {
        Id           = Guid.NewGuid();
        CustomerId   = customerId;
        Currency     = currency;
        Status       = OrderStatus.Placed;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Order Place(Guid customerId, string currencyRaw, IReadOnlyCollection<OrderItemDraft> items)
    {
        OrderGuard.HasItems(items);

        var order = new Order(customerId, Currency.Create(currencyRaw));

        foreach (var item in items)
        {
            OrderGuard.QuantityIsPositive(item.Quantity);
            order._items.Add(OrderItem.Create(item.ProductId, item.UnitPrice, item.Quantity));
        }

        return order;
    }

    public OrderConfirmedDomainEvent Confirm()
    {
        OrderGuard.CanConfirm(Status);

        Status         = OrderStatus.Confirmed;
        ConfirmedAtUtc = DateTime.UtcNow;

        return new OrderConfirmedDomainEvent(
            Id, _items.Select(i => new OrderStockAdjustment(i.ProductId, i.Quantity)).ToList());
    }
}
