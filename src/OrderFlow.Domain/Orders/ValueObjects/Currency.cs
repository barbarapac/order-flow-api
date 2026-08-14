using System.Text.RegularExpressions;
using OrderFlow.Domain.Orders.Exceptions;

namespace OrderFlow.Domain.Orders.ValueObjects;

public sealed partial record Currency
{
    private Currency(string value) => Value = value;

    public string Value { get; }

    public static Currency Create(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw OrderException.CurrencyRequired();
        }

        var normalized = rawValue.Trim().ToUpperInvariant();

        if (!CurrencyRegex().IsMatch(normalized))
        {
            throw OrderException.CurrencyInvalidFormat(rawValue);
        }

        return new Currency(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z]{3}$")]
    private static partial Regex CurrencyRegex();
}
