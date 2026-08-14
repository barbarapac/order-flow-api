using System.Security.Claims;
using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Application.Orders.GetAll;
using OrderFlow.Domain.Orders;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Orders.GetAll;

public sealed class GetAllOrdersEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken,
                OrderStatus? status = null,
                int page = 1,
                int pageSize = 20) =>
            {
                var query  = new GetAllOrdersQuery(user.GetUserId(), status, page, pageSize);
                var result = await sender.Send(query, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("GetAllOrders")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<PagedResult<GetAllOrdersResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }
}
