using Mediator;
using OrderFlow.Application.Products.Create;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Products.Create;

public sealed class CreateProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CreateProductCommand(request.Name, request.UnitPrice, request.AvailableQuantity);
                var result  = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/products/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("CreateProduct")
            .WithTags("Products")
            .RequireAuthorization()
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}
