using Mediator;

namespace OrderFlow.Application.Products.GetAll;

public sealed record GetAllProductsQuery : IQuery<IReadOnlyCollection<GetAllProductsResponse>>;
