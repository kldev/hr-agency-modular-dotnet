using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public class TextSummary
{
    public const int MaxLength = 5000;
    
    public const string MaxLengthMessage =
        "Note cannot exceed 5000 characters.";

    private TextSummary(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TextSummary Create(string value)
    {
        var (title, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : title!;
    }

    public static (TextSummary? title, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (new TextSummary(string.Empty), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new TextSummary(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}