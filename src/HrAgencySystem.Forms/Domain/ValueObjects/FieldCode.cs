using System.Text.RegularExpressions;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Forms.Domain.ValueObjects;

/// <summary>
/// The stable name of a field - what a report asks by and what outlives every relabelling.
/// Dotted camelCase segments (<c>employee.firstName</c>, <c>gdpr.consent</c>), because that is how
/// the people defining forms already write them.
/// <para>
/// The dots are why the front end never uses a code as a form field name: TanStack Form reads a dot
/// as a path. See <c>features/forms/schema/fieldKeys.ts</c>.
/// </para>
/// </summary>
public sealed partial record FieldCode
{
    public const int MaxLength = 64;

    /// <summary>The catalogue's namespace. A form's own field cannot take it, or a report asking
    /// for <c>employee.pesel</c> could not tell the person's number from a form's local copy.</summary>
    public const string SystemPrefix = "employee.";

    public const string RequiredMessage = "A field needs a code.";
    public const string MaxLengthMessage = "A field code cannot exceed 64 characters.";

    public const string FormatMessage =
        "A field code is made of camelCase words separated by dots, e.g. gdpr.consent.";

    public const string ReservedPrefixMessage =
        "Codes starting with 'employee.' belong to system fields. Pick the field from the catalogue instead.";

    private FieldCode(string value) => Value = value;

    public string Value { get; }

    public bool IsSystemNamespace => Value.StartsWith(SystemPrefix, StringComparison.Ordinal);

    public static FieldCode Create(string? value)
    {
        var (code, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : code!;
    }

    public static (FieldCode? code, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        if (!Format().IsMatch(normalized))
            return (null, FormatMessage);

        return (new FieldCode(normalized), null);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z][a-zA-Z0-9]*(\\.[a-z][a-zA-Z0-9]*)*$")]
    private static partial Regex Format();
}
