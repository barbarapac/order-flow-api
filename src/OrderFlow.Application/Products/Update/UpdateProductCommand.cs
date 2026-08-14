using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Products.Update;

public sealed record UpdateProductCommand(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity)
    : ICommand<Result<UpdateProductResponse>>;
