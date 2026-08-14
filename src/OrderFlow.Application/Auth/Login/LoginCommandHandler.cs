using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Users;

namespace OrderFlow.Application.Auth.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("auth.invalid_credentials", "E-mail ou senha inválidos.");

    public async ValueTask<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(InvalidCredentials);
        }

        var (token, expiresAtUtc) = jwtTokenGenerator.Generate(user);

        return Result<LoginResponse>.Success(
            new LoginResponse(token, expiresAtUtc, user.Id, user.Name, user.Email.Value));
    }
}
