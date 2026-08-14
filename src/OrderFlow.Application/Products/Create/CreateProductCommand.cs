using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Products.Create;

public sealed record CreateProductCommand(string Name, decimal UnitPrice, int AvailableQuantity)
    : ICommand<Result<CreateProductResponse>>;
