using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.LegalEntities.Domain.ValueObjects;

/// <summary>
/// Serves both the trading name and the registered one, which differ in what they are for rather
/// than in what they may contain: "HR Agency" against "HR Agency spółka z ograniczoną
/// odpowiedzialnością". The registered form is the longer of the two, so the limit follows it.
/// </summary>
public sealed record LegalEntityName
{
    private const int MaxLength = 250;

    public const string RequiredMessage = "Legal entity name is required.";
    public const string MaxLengthMessage = "Legal entity name cannot exceed 250 characters.";

    private LegalEntityName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LegalEntityName Create(string value)
    {
        var (name, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : name!;
    }

    public static (LegalEntityName? name, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new LegalEntityName(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}
