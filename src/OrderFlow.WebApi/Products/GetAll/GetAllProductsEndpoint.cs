using Mediator;
using OrderFlow.Application.Products.GetAll;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Products.GetAll;

public sealed class GetAllProductsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var products = await sender.Send(new GetAllProductsQuery(), cancellationToken);
                return Results.Ok(products);
            })
            .WithName("GetAllProducts")
            .WithTags("Products")
            .RequireAuthorization()
            .Produces<IReadOnlyCollection<GetAllProductsResponse>>(StatusCodes.Status200OK);
    }
}
