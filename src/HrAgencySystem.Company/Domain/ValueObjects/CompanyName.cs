namespace HrAgencySystem.Company.Domain.ValueObjects;

public sealed record CompanyName
{
    private const int MaxLength = 250;
    public const string RequiredMessage = "Company name is required.";
    public const string MaxLengthMessage = "Company name cannot exceed 250 characters.";

    private CompanyName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CompanyName Create(string value)
    {
        var (companyName, error) = TryCreate(value);

        return error is not null ? throw new ArgumentException(error, nameof(value)) : companyName!;
    }

    public static (CompanyName? companyName, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null,
                MaxLengthMessage);

        return (new CompanyName(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}