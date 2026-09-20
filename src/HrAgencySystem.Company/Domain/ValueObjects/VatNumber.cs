using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Company.Domain.ValueObjects;

/// <summary>
/// An EU VAT identification number (<c>PL1234567890</c>, <c>BE0123456789</c>, <c>DE123456789</c>).
/// <para>
/// A different thing from <see cref="TaxId"/>, which holds the national number, and deliberately not
/// validated beyond its shape: the per country rules differ, change, and are checked by VIES rather
/// than by us. What matters here is that a two letter country prefix is present, because that is the
/// part a person reading an invoice relies on.
/// </para>
/// </summary>
public sealed record VatNumber
{
    public const int MaxLength = 20;
    public const string InvalidFormatMessage =
        "VAT number must start with a two letter country code followed by 2-12 alphanumeric characters.";

    private VatNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static VatNumber Create(string? value)
    {
        var (vatNumber, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : vatNumber!;
    }

    /// <summary>Blank is allowed: not every company we record has one, or has given it to us yet.</summary>
    public static (VatNumber? value, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (new VatNumber(string.Empty), null);

        var normalized = value.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        if (normalized.Length is < 4 or > MaxLength)
            return (null, InvalidFormatMessage);

        if (!char.IsAsciiLetterUpper(normalized[0]) || !char.IsAsciiLetterUpper(normalized[1]))
            return (null, InvalidFormatMessage);

        if (!normalized[2..].All(char.IsAsciiLetterOrDigit))
            return (null, InvalidFormatMessage);

        return (new VatNumber(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}
