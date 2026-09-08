using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record WebSite
{
    public const int MaxLength = 250;

    public const string MaxLengthMessage =
        "Website url cannot exceed 250 characters.";

    private WebSite(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static WebSite Create(string value)
    {
        var (website, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : website!;
    }

    public static (WebSite? website, string? error) TryCreate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (new WebSite(string.Empty), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new WebSite(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}