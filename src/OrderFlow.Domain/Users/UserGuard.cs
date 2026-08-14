using OrderFlow.Domain.Users.Exceptions;

namespace OrderFlow.Domain.Users;

internal static class UserGuard
{
    public static void NameIsValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw UserException.InvalidName();
    }

    public static void PasswordHashIsValid(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw UserException.InvalidPasswordHash();
    }
}
