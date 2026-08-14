using OrderFlow.Domain._Shared;

namespace OrderFlow.Domain.Users;

public sealed class UserDomainException : DomainException
{
    private UserDomainException(string code, string message, ErrorType type) : base(code, message, type) { }

    public static UserDomainException InvalidName() =>
        new("user.invalid_name", "User name cannot be empty.", ErrorType.Validation);

    public static UserDomainException InvalidPasswordHash() =>
        new("user.invalid_password_hash", "Password hash cannot be empty.", ErrorType.Validation);

    public static UserDomainException EmailRequired() =>
        new("user.invalid_email", "Email cannot be empty.", ErrorType.Validation);

    public static UserDomainException EmailInvalidFormat(string rawValue) =>
        new("user.invalid_email", $"'{rawValue}' is not a valid email address.", ErrorType.Validation);
}
