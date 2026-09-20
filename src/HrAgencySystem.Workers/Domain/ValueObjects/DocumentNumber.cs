using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Workers.Domain.ValueObjects;

/// <summary>
/// The number on a document - an identity card, a passport, a work permit.
/// <para>
/// A type of its own mainly so there is one place to say this: the value is personal data. It is
/// written down where somebody has to read it off - the file and the screen that shows the file -
/// and nowhere else. Never in a log line, never in a suggestion list and never in an error message,
/// which is why the failures below describe the field and never quote it.
/// </para>
/// </summary>
public sealed record DocumentNumber
{
    private const int MaxLength = 50;

    public const string RequiredMessage = "Document number is required.";
    public const string MaxLengthMessage = "Document number cannot exceed 50 characters.";

    private DocumentNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DocumentNumber Create(string value)
    {
        var (number, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : number!;
    }

    public static (DocumentNumber? number, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new DocumentNumber(normalized), null);
    }

    public override string ToString() => Value;
}
