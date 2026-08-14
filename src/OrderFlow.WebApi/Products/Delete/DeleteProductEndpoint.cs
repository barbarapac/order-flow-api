using Mediator;
using OrderFlow.Application.Products.Delete;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Products.Delete;

public sealed class DeleteProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.Error!.ToProblemResult();
            })
            .WithName("DeleteProduct")
            .WithTags("Products")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
