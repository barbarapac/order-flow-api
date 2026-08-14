namespace OrderFlow.Domain.Orders;

public sealed class OrderItem
{
    public Guid Id             { get; private set; }
    public Guid ProductId      { get; private set; }
    public decimal UnitPrice   { get; private set; }
    public int Quantity        { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem(Guid productId, decimal unitPrice, int quantity)
    {
        Id        = Guid.NewGuid();
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity  = quantity;
    }

    internal static OrderItem Create(Guid productId, decimal unitPrice, int quantity)
    {
        return new OrderItem(productId, unitPrice, quantity);
    }
}
