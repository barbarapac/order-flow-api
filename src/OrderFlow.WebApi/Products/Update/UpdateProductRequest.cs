namespace OrderFlow.WebApi.Products.Update;

public sealed record UpdateProductRequest(string Name, decimal UnitPrice, int AvailableQuantity);
