using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Users;

namespace OrderFlow.Application.Users.Register;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    public async ValueTask<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailAlreadyRegistered = await userRepository.ExistsByEmailAsync(request.Email, cancellationToken);

        if (emailAlreadyRegistered)
        {
            return Result<RegisterUserResponse>.Failure(
                Error.Conflict("user.email_already_registered", $"O e-mail '{request.Email}' já está cadastrado."));
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user         = User.Register(request.Name, request.Email, passwordHash);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RegisterUserResponse>.Success(new RegisterUserResponse(user.Id, user.Name, user.Email.Value, user.CreatedAtUtc));
    }
}
