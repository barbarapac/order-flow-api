using Mediator;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>;
