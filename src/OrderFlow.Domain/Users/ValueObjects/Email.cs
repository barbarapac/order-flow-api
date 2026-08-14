using System.Text.RegularExpressions;
using OrderFlow.Domain.Users.Exceptions;

namespace OrderFlow.Domain.Users.ValueObjects;

public sealed partial record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw UserException.EmailRequired();
        }

        var normalized = rawValue.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
        {
            throw UserException.EmailInvalidFormat(rawValue);
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
