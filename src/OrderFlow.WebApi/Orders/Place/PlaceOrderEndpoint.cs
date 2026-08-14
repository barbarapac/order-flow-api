using System.Security.Claims;
using Mediator;
using OrderFlow.Application.Orders.Place;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Orders.Place;

public sealed class PlaceOrderEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (PlaceOrderRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new PlaceOrderCommand(
                    user.GetUserId(),
                    request.Currency,
                    request.Items.Select(i => new PlaceOrderItemRequest(i.ProductId, i.Quantity)).ToList());

                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/orders/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("PlaceOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<PlaceOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
