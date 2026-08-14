using OrderFlow.Domain._Shared;

namespace OrderFlow.Domain.Users.Exceptions;

public sealed class UserException : DomainException
{
    private UserException(string code, string message, ErrorType type) : base(code, message, type) { }

    public static UserException InvalidName() =>
        new("user.invalid_name", "O nome do usuário não pode ser vazio.", ErrorType.Validation);

    public static UserException InvalidPasswordHash() =>
        new("user.invalid_password_hash", "O hash da senha não pode ser vazio.", ErrorType.Validation);

    public static UserException EmailRequired() =>
        new("user.invalid_email", "O e-mail não pode ser vazio.", ErrorType.Validation);

    public static UserException EmailInvalidFormat(string rawValue) =>
        new("user.invalid_email", $"'{rawValue}' não é um endereço de e-mail válido.", ErrorType.Validation);
}
