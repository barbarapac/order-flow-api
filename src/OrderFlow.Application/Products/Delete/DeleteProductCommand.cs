using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Products.Delete;

public sealed record DeleteProductCommand(Guid Id) : ICommand<Result>;
