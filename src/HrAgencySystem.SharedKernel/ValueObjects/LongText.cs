using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record LongText
{
    public const int MaxLength = 5000;
    
    public const string MaxLengthMessage =
        "Text cannot exceed 5000 characters.";

    private LongText(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LongText Create(string value)
    {
        var (title, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : title!;
    }

    public static string FieldIsRequired(string fieldName)
        => $"{fieldName} is required.";
    
    public static (LongText? title, string? error) TryCreate(
        string value, bool isRequired = false, string fieldName = "")
    {
        if (isRequired)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (null, FieldIsRequired(fieldName));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(value))
                return (new LongText(string.Empty), null);
        }

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new LongText(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}