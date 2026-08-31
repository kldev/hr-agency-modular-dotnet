using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Company.Domain.ValueObjects;

public sealed record TaxId
{
    private const int MaxLength = 50;
    public const string RequiredMessage = "Tax ID is required.";
    public const string MaxLenghtMessage = "Tax ID cannot exceed 50 characters.";

    private TaxId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TaxId Create(string value)
    {
        var (taxId, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : taxId!;
    }

    public static (TaxId? taxId, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength) return (null, MaxLenghtMessage);

        return (new TaxId(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}