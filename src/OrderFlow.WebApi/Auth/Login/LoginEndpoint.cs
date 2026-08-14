using Mediator;
using OrderFlow.Application.Auth.Login;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Auth.Login;

public sealed class LoginEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new LoginCommand(request.Email, request.Password);
                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("Login")
            .WithTags("Auth")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();
    }
}
