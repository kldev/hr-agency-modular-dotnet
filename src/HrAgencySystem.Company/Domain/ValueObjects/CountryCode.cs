using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Company.Domain.ValueObjects;

public sealed record CountryCode
{
    public const string InvalidFormatMessage = "Country code must be ISO 3166-1 alpha-2.";
    public const string OnlyCharactersAllowedMessage = "Country code must contain only letters.";
    public const string RequiredMessage = "Country code is required.";

    private CountryCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CountryCode Create(string value)
    {
        var (countryCode, error) = TryCreate(value);
        return error is not null ? throw new InValidValueException(error) : countryCode!;
    }

    public static (CountryCode? countryCode, string? error) TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, RequiredMessage);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 2) return (null, InvalidFormatMessage);

        if (!normalized.All(char.IsLetter))
            return (null, OnlyCharactersAllowedMessage);

        return (new CountryCode(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}