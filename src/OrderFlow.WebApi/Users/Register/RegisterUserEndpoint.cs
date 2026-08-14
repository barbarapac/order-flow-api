using MediatR;
using OrderFlow.Application.Users.Register;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi.Users.Register;

public sealed class RegisterUserEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (RegisterUserRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RegisterUserCommand(request.Name, request.Email, request.Password);
                var result  = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/users/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblemResult();
            })
            .WithName("RegisterUser")
            .WithTags("Users")
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
    }
}
