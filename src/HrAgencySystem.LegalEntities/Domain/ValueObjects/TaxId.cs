using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.LegalEntities.Domain.ValueObjects;

/// <summary>
/// The national tax number of one of our own entities. A deliberate copy of the client side
/// <c>Company.TaxId</c> rather than a type promoted into SharedKernel: forty lines duplicated cost
/// less than a shared type that two modules then have to agree about forever.
/// </summary>
public sealed record TaxId
{
    private const int MaxLength = 50;

    public const string RequiredMessage = "Tax ID is required.";
    public const string MaxLengthMessage = "Tax ID cannot exceed 50 characters.";

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

    public static (TaxId? taxId, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new TaxId(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}
