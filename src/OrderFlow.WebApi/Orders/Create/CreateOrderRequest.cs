namespace OrderFlow.WebApi.Orders.Create;

public sealed record CreateOrderRequest(string Currency, IReadOnlyCollection<CreateOrderItemDto> Items);

public sealed record CreateOrderItemDto(Guid ProductId, int Quantity);
