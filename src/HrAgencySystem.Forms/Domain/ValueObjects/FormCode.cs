using System.Text.RegularExpressions;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Forms.Domain.ValueObjects;

/// <summary>
/// A form's stable name within the organization (<c>gdpr-consent</c>), unique through
/// <c>FormCodeReservation</c>. It never changes: the name is for people, the code is for everything
/// that has to find the form again - reports, a seeder, a future integration.
/// </summary>
public sealed partial record FormCode
{
    public const int MaxLength = 64;

    public const string RequiredMessage = "A form needs a code.";
    public const string MaxLengthMessage = "A form code cannot exceed 64 characters.";

    public const string FormatMessage =
        "A form code is lower-case letters, digits and hyphens, starting with a letter, e.g. gdpr-consent.";

    private FormCode(string value) => Value = value;

    public string Value { get; }

    public static FormCode Create(string? value)
    {
        var (code, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : code!;
    }

    public static (FormCode? code, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        if (!Format().IsMatch(normalized))
            return (null, FormatMessage);

        return (new FormCode(normalized), null);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z][a-z0-9]*(-[a-z0-9]+)*$")]
    private static partial Regex Format();
}
