using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Application.Products.GetAll;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Products.GetAll;

public sealed class GetAllProductsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
                ISender sender,
                CancellationToken cancellationToken,
                int page = 1,
                int pageSize = 20) =>
            {
                var query  = new GetAllProductsQuery(page, pageSize);
                var result = await sender.Send(query, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("GetAllProducts")
            .WithTags("Products")
            .RequireAuthorization()
            .Produces<PagedResult<GetAllProductsResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }
}
