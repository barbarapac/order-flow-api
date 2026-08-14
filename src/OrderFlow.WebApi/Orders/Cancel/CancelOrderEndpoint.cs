using System.Security.Claims;
using Mediator;
using OrderFlow.Application.Orders.Cancel;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Orders.Cancel;

public sealed class CancelOrderEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{id:guid}/cancel", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CancelOrderCommand(id, user.GetUserId());

                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("CancelOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<CancelOrderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
