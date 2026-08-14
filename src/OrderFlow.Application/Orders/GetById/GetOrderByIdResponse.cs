namespace OrderFlow.Application.Orders.GetById;

public sealed record GetOrderByIdResponse
{
    public Guid Id                  { get; init; }
    public Guid CustomerId          { get; init; }
    public string Currency          { get; init; } = string.Empty;
    public string Status            { get; init; } = string.Empty;
    public decimal Total            { get; init; }
    public DateTime CreatedAtUtc    { get; init; }
    public DateTime? ConfirmedAtUtc { get; init; }
    public DateTime? CanceledAtUtc  { get; init; }
    public IReadOnlyCollection<GetOrderByIdItemResponse> Items { get; init; } = [];
}

public sealed record GetOrderByIdItemResponse
{
    public Guid ProductId    { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity      { get; init; }
    public decimal LineTotal { get; init; }
}
