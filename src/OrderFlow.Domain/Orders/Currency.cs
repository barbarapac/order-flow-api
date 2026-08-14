using System.Text.RegularExpressions;

namespace OrderFlow.Domain.Orders;

public sealed partial record Currency
{
    private Currency(string value) => Value = value;

    public string Value { get; }

    public static Currency Create(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw OrderDomainException.CurrencyRequired();
        }

        var normalized = rawValue.Trim().ToUpperInvariant();

        if (!CurrencyRegex().IsMatch(normalized))
        {
            throw OrderDomainException.CurrencyInvalidFormat(rawValue);
        }

        return new Currency(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z]{3}$")]
    private static partial Regex CurrencyRegex();
}
