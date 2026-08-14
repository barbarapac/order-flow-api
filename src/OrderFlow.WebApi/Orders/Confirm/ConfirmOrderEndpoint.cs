using System.Security.Claims;
using Mediator;
using OrderFlow.Application.Orders.Confirm;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Orders.Confirm;

public sealed class ConfirmOrderEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{id:guid}/confirm", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new ConfirmOrderCommand(id, user.GetUserId());

                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("ConfirmOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<ConfirmOrderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
