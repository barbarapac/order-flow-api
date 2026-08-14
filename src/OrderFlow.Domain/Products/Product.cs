namespace OrderFlow.Domain.Products;

public sealed class Product
{
    public Guid Id                     { get; private set; } = Guid.NewGuid();
    public string Name                 { get; set; } = default!;
    public decimal UnitPrice           { get; set; }
    public int AvailableQuantity       { get; set; }
    public DateTime CreatedAtUtc       { get; private set; } = DateTime.UtcNow;
}
