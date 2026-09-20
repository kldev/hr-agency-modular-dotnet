using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.LegalEntities.Domain.ValueObjects;

/// <summary>
/// An EU VAT identification number of one of our own entities. Same reasoning and same shape check
/// as the client side one: the per country rules differ and are checked by VIES, not by us, so only
/// the two letter prefix - the part a person reading an invoice relies on - is required.
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

    /// <summary>Blank is allowed: a domestic-only entity has no EU number.</summary>
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
