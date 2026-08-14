using Mediator;
using OrderFlow.Application.Products.Update;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Products.Update;

public sealed class UpdateProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new UpdateProductCommand(id, request.Name, request.UnitPrice, request.AvailableQuantity);
                var result  = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("UpdateProduct")
            .WithTags("Products")
            .RequireAuthorization()
            .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
    }
}
