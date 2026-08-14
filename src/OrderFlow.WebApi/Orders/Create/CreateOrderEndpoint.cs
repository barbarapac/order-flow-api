using System.Security.Claims;
using Mediator;
using OrderFlow.Application.Orders.Create;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Orders.Create;

public sealed class CreateOrderEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (CreateOrderRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CreteOrderCommand(
                    user.GetUserId(),
                    request.Currency,
                    request.Items.Select(i => new CreateOrderItemRequest(i.ProductId, i.Quantity)).ToList());

                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/orders/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("PlaceOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
