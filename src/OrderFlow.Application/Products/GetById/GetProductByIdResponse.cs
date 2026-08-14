namespace OrderFlow.Application.Products.GetById;

public sealed record GetProductByIdResponse
{
    public Guid Id               { get; init; }
    public string Name           { get; init; } = string.Empty;
    public decimal UnitPrice     { get; init; }
    public int AvailableQuantity { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
