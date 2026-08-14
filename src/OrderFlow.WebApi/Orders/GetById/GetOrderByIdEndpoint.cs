using System.Security.Claims;
using Mediator;
using OrderFlow.Application.Orders.GetById;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Orders.GetById;

public sealed class GetOrderByIdEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetOrderByIdQuery(id, user.GetUserId()), cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("GetOrderById")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
