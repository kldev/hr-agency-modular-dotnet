using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record ShortNote
{
    public const int MaxLength = 500;

    public const string RequiredMessage = "Note is required.";
    public const string MaxLengthMessage =
        "Note cannot exceed 500 characters.";

    private ShortNote(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ShortNote Create(string value, bool isRequired = true)
    {
        var (title, error) = TryCreate(value, isRequired);

        return error is not null
            ? throw new InValidValueException(error)
            : title!;
    }

    public static (ShortNote? title, string? error) TryCreate(
        string value, bool isRequired = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            return isRequired ? (null, RequiredMessage) 
                : (new ShortNote(""), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new ShortNote(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}