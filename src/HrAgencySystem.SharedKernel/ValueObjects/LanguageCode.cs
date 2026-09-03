using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record LanguageCode
{
    public const string InvalidFormatMessage = "Language code must be ISO 3166-1 alpha-2.";
    public const string OnlyCharactersAllowedMessage = "Language code must contain only letters.";
    public const string RequiredMessage = "Language code is required.";

    private LanguageCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LanguageCode Create(string value)
    {
        var (code, error) = TryCreate(value);
        return error is not null ? throw new InValidValueException(error) : code!;
    }

    public static (LanguageCode? code, string? error) TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, RequiredMessage);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 2) return (null, InvalidFormatMessage);

        if (!normalized.All(char.IsLetter))
            return (null, OnlyCharactersAllowedMessage);

        return (new LanguageCode(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}