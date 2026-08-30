namespace HrAgencySystem.Organization.Domain.ValueObjects;

public sealed record OrganizationName
{
    private const int MaxLength = 250;

    public const string RequiredMessage =
        "Organization name is required.";

    public const string MaxLengthMessage =
        "Organization name cannot exceed 250 characters.";

    private OrganizationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OrganizationName Create(string value)
    {
        var (name, error) = TryCreate(value);

        return error is not null ? throw new ArgumentException(error, nameof(value)) : name!;
    }

    public static (OrganizationName? name, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength) return (null, MaxLengthMessage);

        return (new OrganizationName(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}