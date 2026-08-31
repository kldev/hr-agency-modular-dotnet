using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Organization.Domain.ValueObjects;

public sealed record OrganizationSlug
{
    private const int MaxLength = 100;

    public const string RequiredMessage =
        "Organization slug is required.";

    public const string MaxLengthMessage =
        "Organization slug cannot exceed 100 characters.";

    private OrganizationSlug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OrganizationSlug Create(string value)
    {
        var (slug, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : slug!;
    }

    public static (OrganizationSlug? slug, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, RequiredMessage);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength) return (null, MaxLengthMessage);

        return (new OrganizationSlug(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}