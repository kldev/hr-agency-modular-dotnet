using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Agency.Domain.ValueObjects;

public sealed record OrgUnitName
{
    private const int MaxLength = 100;

    public const string RequiredMessage = "The unit needs a name.";
    public const string MaxLengthMessage = "The unit name cannot exceed 100 characters.";

    private OrgUnitName(string value) => Value = value;

    public string Value { get; }

    public static OrgUnitName Create(string value)
    {
        var (name, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : name!;
    }

    public static (OrgUnitName? name, string? error) TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new OrgUnitName(normalized), null);
    }

    public override string ToString() => Value;
}
