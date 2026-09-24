using System.Globalization;
using System.Text.RegularExpressions;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Values;

namespace HrAgencySystem.Forms.Domain.Validation;

/// <summary>
/// How much of a form has to be right. A draft may be half empty - a long document is filled over
/// several sittings - but whatever <em>is</em> in it has to be well formed. Submitting asks for
/// everything, required fields included.
/// </summary>
public enum ValidationMode
{
    Draft,
    Submit,
}

/// <summary>What is wrong with one field. <see cref="Code"/> is compared by tests, the message shown.</summary>
public sealed record FieldError(string FieldCode, string Code, string Message);

/// <summary>
/// The closed set of things a value can be wrong about. The TypeScript mirror
/// (<c>frontend/src/features/forms/schema/buildAnswerSchema.ts</c>) uses the same codes, and both are
/// held to <c>tests/fixtures/forms-validation-cases.json</c>.
/// </summary>
public static class FieldErrorCodes
{
    public const string UnknownField = "unknownField";
    public const string Duplicate = "duplicate";
    public const string WrongType = "wrongType";
    public const string Required = "required";
    public const string MinLength = "minLength";
    public const string MaxLength = "maxLength";
    public const string Pattern = "pattern";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string Country = "country";
    public const string Min = "min";
    public const string Max = "max";
    public const string Decimals = "decimals";
    public const string MinDate = "minDate";
    public const string MaxDate = "maxDate";
    public const string Option = "option";
    public const string MinSelected = "minSelected";
    public const string MaxSelected = "maxSelected";
}

/// <summary>
/// Checks answers against the layout they were given for - always a frozen version, never the
/// draft, because the draft can change under a response that is half filled.
/// <para>
/// Pure: no I/O, so it runs the same in a handler, in a test and against a golden case. One error
/// per field, in a fixed order (type, required, then the rules), which is what the front end shows
/// under the control and what keeps the two implementations comparable.
/// </para>
/// </summary>
public static partial class FormAnswersValidator
{
    public const int MaxTextLength = 10_000;

    public const string UnknownFieldMessage = "This form has no such field.";
    public const string DuplicateMessage = "The field is answered twice.";
    public const string WrongTypeMessage = "The answer is not of the kind this field takes.";
    public const string RequiredMessage = "This field is required.";
    public const string RequiredConsentMessage = "This box has to be ticked.";
    public const string EmailMessage = "Enter a valid e-mail address.";
    public const string PhoneMessage = "Enter a valid phone number.";
    public const string CountryMessage = "Pick a country.";
    public const string OptionMessage = "Pick one of the listed options.";
    public const string PatternMessage = "The value has the wrong format.";

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromMilliseconds(100);

    public static string MinLengthMessage(int length) => $"Enter at least {length} characters.";

    public static string MaxLengthMessage(int length) => $"Enter at most {length} characters.";

    public static string MinMessage(decimal min) => $"Enter a number no smaller than {Format(min)}.";

    public static string MaxMessage(decimal max) => $"Enter a number no greater than {Format(max)}.";

    public static string DecimalsMessage(int decimals) =>
        decimals == 0 ? "Enter a whole number." : $"Use at most {decimals} decimal places.";

    public static string MinDateMessage(DateOnly date) => $"Pick a date on or after {Format(date)}.";

    public static string MaxDateMessage(DateOnly date) => $"Pick a date on or before {Format(date)}.";

    public static string MinSelectedMessage(int count) => $"Pick at least {count}.";

    public static string MaxSelectedMessage(int count) => $"Pick at most {count}.";

    public static IReadOnlyList<FieldError> Validate(
        IReadOnlyList<FormPage> pages,
        IReadOnlyList<FieldAnswer> answers,
        ValidationMode mode
    )
    {
        var errors = new List<FieldError>();
        var fields = pages.AllFields.ToDictionary(field => field.Code);
        var given = new Dictionary<string, FieldValue>();

        foreach (var answer in answers)
        {
            if (!fields.ContainsKey(answer.FieldCode))
                errors.Add(new FieldError(answer.FieldCode, FieldErrorCodes.UnknownField, UnknownFieldMessage));
            else if (!given.TryAdd(answer.FieldCode, answer.Value))
                errors.Add(new FieldError(answer.FieldCode, FieldErrorCodes.Duplicate, DuplicateMessage));
        }

        foreach (var field in fields.Values)
        {
            var error = ValidateValue(field, given.GetValueOrDefault(field.Code), mode);

            if (error is not null)
                errors.Add(error);
        }

        return errors;
    }

    /// <summary>
    /// One field, one value. Also used for a form field's default value, which has to be a value the
    /// field would accept in a draft.
    /// </summary>
    public static FieldError? ValidateValue(FormField field, FieldValue? value, ValidationMode mode)
    {
        var slot = FieldValue.SlotFor(field.Type);

        if (value is not null && value.HasValueOutside(slot))
            return Error(field, FieldErrorCodes.WrongType, WrongTypeMessage, overridable: false);

        var rules = field.Rules;

        if (value is null || value.IsEmpty || (field.Type == FieldType.Boolean && value.Boolean is false))
        {
            if (mode == ValidationMode.Submit && rules.Required)
            {
                var message = field.Type == FieldType.Boolean ? RequiredConsentMessage : RequiredMessage;

                return Error(field, FieldErrorCodes.Required, message);
            }

            return null;
        }

        return field.Type switch
        {
            FieldType.Number => ValidateNumber(field, value.Number!.Value),
            FieldType.Date => ValidateDate(field, value.Date!.Value),
            FieldType.Boolean => null,
            FieldType.SingleChoice => ValidateOption(field, value.Text!.Trim()),
            FieldType.MultiChoice => ValidateSelection(field, value.Values!),
            FieldType.Country => ValidateCountry(field, value.Text!.Trim()),
            _ => ValidateText(field, value.Text!.Trim()),
        };
    }

    private static FieldError? ValidateText(FormField field, string text)
    {
        var rules = field.Rules;

        if (rules.MinLength is { } min && text.Length < min)
            return Error(field, FieldErrorCodes.MinLength, MinLengthMessage(min));

        var max = rules.MaxLength ?? MaxTextLength;

        if (text.Length > max)
            return Error(field, FieldErrorCodes.MaxLength, MaxLengthMessage(max));

        if (field.Type == FieldType.Email && !EmailFormat().IsMatch(text))
            return Error(field, FieldErrorCodes.Email, EmailMessage);

        if (field.Type == FieldType.Phone && !IsPhone(text))
            return Error(field, FieldErrorCodes.Phone, PhoneMessage);

        if (rules.Pattern is { } pattern && !MatchesWhole(pattern, text))
            return Error(field, FieldErrorCodes.Pattern, PatternMessage);

        return null;
    }

    private static FieldError? ValidateNumber(FormField field, decimal number)
    {
        var rules = field.Rules;

        if (rules.Min is { } min && number < min)
            return Error(field, FieldErrorCodes.Min, MinMessage(min));

        if (rules.Max is { } max && number > max)
            return Error(field, FieldErrorCodes.Max, MaxMessage(max));

        if (rules.Decimals is { } decimals && decimal.Round(number, decimals) != number)
            return Error(field, FieldErrorCodes.Decimals, DecimalsMessage(decimals));

        return null;
    }

    private static FieldError? ValidateDate(FormField field, DateOnly date)
    {
        var rules = field.Rules;

        if (rules.MinDate is { } min && date < min)
            return Error(field, FieldErrorCodes.MinDate, MinDateMessage(min));

        if (rules.MaxDate is { } max && date > max)
            return Error(field, FieldErrorCodes.MaxDate, MaxDateMessage(max));

        return null;
    }

    private static FieldError? ValidateOption(FormField field, string value) =>
        field.Options.Any(option => option.Value == value)
            ? null
            : Error(field, FieldErrorCodes.Option, OptionMessage, overridable: false);

    private static FieldError? ValidateSelection(FormField field, IReadOnlyList<string> values)
    {
        if (values.Any(value => field.Options.All(option => option.Value != value)))
            return Error(field, FieldErrorCodes.Option, OptionMessage, overridable: false);

        var rules = field.Rules;

        if (rules.MinSelected is { } min && values.Count < min)
            return Error(field, FieldErrorCodes.MinSelected, MinSelectedMessage(min));

        if (rules.MaxSelected is { } max && values.Count > max)
            return Error(field, FieldErrorCodes.MaxSelected, MaxSelectedMessage(max));

        return null;
    }

    private static FieldError? ValidateCountry(FormField field, string value) =>
        CountryFormat().IsMatch(value)
            ? null
            : Error(field, FieldErrorCodes.Country, CountryMessage, overridable: false);

    /// <summary>
    /// The pattern has to match the whole value, on both sides - JavaScript's <c>test</c> would
    /// otherwise accept a PESEL with a letter tacked on. A pattern that does not compile, or runs
    /// past the timeout, counts as not matching; the layout policy refuses such a pattern long
    /// before an answer meets it.
    /// </summary>
    internal static bool MatchesWhole(string pattern, string text)
    {
        try
        {
            return Regex.IsMatch(text, $"^(?:{pattern})$", RegexOptions.CultureInvariant, PatternTimeout);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    internal static bool IsValidPattern(string pattern)
    {
        try
        {
            _ = new Regex($"^(?:{pattern})$", RegexOptions.CultureInvariant, PatternTimeout);

            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>Six to fifteen digits, optionally led by a plus, with spaces, dashes and brackets in between.</summary>
    private static bool IsPhone(string text)
    {
        if (!PhoneFormat().IsMatch(text))
            return false;

        var digits = text.Count(char.IsAsciiDigit);

        return digits is >= 6 and <= 15;
    }

    private static FieldError Error(
        FormField field,
        string code,
        string message,
        bool overridable = true
    ) =>
        new(
            field.Code,
            code,
            overridable && !string.IsNullOrWhiteSpace(field.Rules.Message) ? field.Rules.Message : message
        );

    private static string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Format(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    [GeneratedRegex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")]
    private static partial Regex EmailFormat();

    [GeneratedRegex("^\\+?[0-9 ()-]+$")]
    private static partial Regex PhoneFormat();

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex CountryFormat();
}
