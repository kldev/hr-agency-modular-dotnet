using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record EntryText
{
    public const int MaxLength = 4000;

    public const string RequiredMessage = "Entry text is required.";
    public const string MaxLengthMessage =
        "Entry text cannot exceed 4000 characters.";

    private EntryText(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EntryText Create(string value)
    {
        var (entryText, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : entryText!;
    }

    public static List<EntryText> Create(IReadOnlyList<string> values)
    {
        return [.. values.Select(Create)];
    }

    public static (EntryText? entryText, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new EntryText(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}