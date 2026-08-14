using Mediator;
using OrderFlow.Application._Shared;

namespace OrderFlow.Application.Products.GetAll;

public sealed record GetAllProductsQuery(int Page, int PageSize) : IQuery<PagedResult<GetAllProductsResponse>>;
