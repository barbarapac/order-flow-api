using System.Text.RegularExpressions;

namespace OrderFlow.Domain.Users;

public sealed partial record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw UserDomainException.EmailRequired();
        }

        var normalized = rawValue.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
        {
            throw UserDomainException.EmailInvalidFormat(rawValue);
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
