using Mediator;
using OrderFlow.Application.Products.GetById;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Products.GetById;

public sealed class GetProductByIdEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("GetProductById")
            .WithTags("Products")
            .RequireAuthorization()
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
