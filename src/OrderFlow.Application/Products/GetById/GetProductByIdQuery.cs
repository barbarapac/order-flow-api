using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Products.GetById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<Result<GetProductByIdResponse>>;
