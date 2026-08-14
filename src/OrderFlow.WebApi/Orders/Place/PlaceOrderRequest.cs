namespace OrderFlow.WebApi.Orders.Place;

public sealed record PlaceOrderRequest(string Currency, IReadOnlyCollection<PlaceOrderItemDto> Items);

public sealed record PlaceOrderItemDto(Guid ProductId, int Quantity);
