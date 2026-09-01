using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record CurrencyCode
{
    public const int Length = 3;

    public const string RequiredMessage =
        "Currency code is required.";

    public const string InvalidMessage =
        "Currency code must be a valid three-letter ISO 4217 code.";

    private CurrencyCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CurrencyCode Create(string value)
    {
        var (currencyCode, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : currencyCode!;
    }

    public static (CurrencyCode? currencyCode, string? error) TryCreate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != Length ||
            normalized.Any(c => c is < 'A' or > 'Z'))
        {
            return (null, InvalidMessage);
        }

        return (new CurrencyCode(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}