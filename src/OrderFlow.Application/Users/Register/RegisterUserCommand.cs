using MediatR;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Users.Register;

public sealed record RegisterUserCommand(string Name, string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;
