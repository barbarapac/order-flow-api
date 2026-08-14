namespace OrderFlow.WebApi.Products.Create;

public sealed record CreateProductRequest(string Name, decimal UnitPrice, int AvailableQuantity);
