using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Users.Register;

public sealed record RegisterUserCommand(string Name, string Email, string Password)
    : ICommand<Result<RegisterUserResponse>>;
