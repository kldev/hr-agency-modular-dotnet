using HrAgencySystem.Forms.Domain.Validation;

namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// Whether a field's rules and options make sense for its type. Shared by the layout of a form and
/// by the catalogue of system fields, since both define fields.
/// <para>
/// A rule the type cannot use is refused rather than ignored: a stored rule should always be one
/// that is enforced, or the builder would show a "max 5" nobody checks.
/// </para>
/// </summary>
public static class FieldRulesPolicy
{
    public const int MaxOptions = 200;
    public const int MaxOptionLength = 200;
    public const int MaxPatternLength = 500;
    public const int MaxMessageLength = 300;
    public const int MaxDecimals = 6;

    public const string LengthNotForTypeMessage =
        "Length and pattern rules apply to typed text only.";
    public const string RangeNotForTypeMessage =
        "Minimum, maximum and decimals apply to numbers only.";
    public const string DatesNotForTypeMessage = "Date limits apply to dates only.";
    public const string SelectionNotForTypeMessage =
        "Selection limits apply to multiple choice only.";
    public const string NegativeLengthMessage = "Lengths cannot be negative.";
    public const string LengthRangeMessage = "The minimum length is greater than the maximum.";
    public const string NumberRangeMessage = "The minimum is greater than the maximum.";
    public const string DateRangeMessage = "The earliest date is after the latest.";
    public const string SelectionRangeMessage =
        "The minimum selection is greater than the maximum.";
    public const string NegativeSelectionMessage = "Selection limits cannot be negative.";
    public const string SelectionAboveOptionsMessage =
        "More selections are required than there are options.";
    public const string InvalidPatternMessage = "The pattern is not a valid regular expression.";
    public const string PatternTooLongMessage = "The pattern cannot exceed 500 characters.";
    public const string MessageTooLongMessage = "The error message cannot exceed 300 characters.";
    public const string DecimalsRangeMessage = "Decimals must be between 0 and 6.";
    public const string TooManyOptionsMessage = "A field cannot offer more than 200 options.";
    public const string EmptyOptionMessage = "Every option needs a value and a label.";
    public const string OptionTooLongMessage = "An option cannot exceed 200 characters.";
    public const string DuplicateOptionMessage = "Two options share the same value.";

    public static IReadOnlyList<string> Check(
        FieldType type,
        FieldRules rules,
        IReadOnlyList<ChoiceOption> options
    )
    {
        var errors = new List<string>();

        CheckApplicability(type, rules, errors);
        CheckRanges(rules, errors);
        CheckOptions(type, rules, options, errors);

        if (rules.Pattern is { } pattern)
        {
            if (pattern.Length > MaxPatternLength)
                errors.Add(PatternTooLongMessage);
            else if (!FormAnswersValidator.IsValidPattern(pattern))
                errors.Add(InvalidPatternMessage);
        }

        if (rules.Message is { Length: > MaxMessageLength })
            errors.Add(MessageTooLongMessage);

        return errors;
    }

    /// <summary>
    /// Drops what a type cannot hold instead of letting the builder trip over it: options on a text
    /// field left behind after its type was changed, blank strings standing for "no rule".
    /// </summary>
    public static (FieldRules rules, IReadOnlyList<ChoiceOption> options) Normalize(
        FieldType type,
        FieldRules? rules,
        IReadOnlyList<ChoiceOption>? options
    )
    {
        var normalizedRules = (rules ?? FieldRules.None) with
        {
            Pattern = string.IsNullOrWhiteSpace(rules?.Pattern) ? null : rules.Pattern.Trim(),
            Message = string.IsNullOrWhiteSpace(rules?.Message) ? null : rules.Message.Trim(),
        };

        IReadOnlyList<ChoiceOption> normalizedOptions = type.HasOptions
            ?
            [
                .. (options ?? []).Select(option => new ChoiceOption(
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    (option.Value ?? "").Trim(),
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    (option.Label ?? "").Trim()
                )),
            ]
            : [];

        return (normalizedRules, normalizedOptions);
    }

    private static void CheckApplicability(FieldType type, FieldRules rules, List<string> errors)
    {
        if (
            !type.TakesTypedText
            && (
                rules.MinLength is not null
                || rules.MaxLength is not null
                || rules.Pattern is not null
            )
        )
            errors.Add(LengthNotForTypeMessage);

        if (
            type != FieldType.Number
            && (rules.Min is not null || rules.Max is not null || rules.Decimals is not null)
        )
            errors.Add(RangeNotForTypeMessage);

        if (type != FieldType.Date && (rules.MinDate is not null || rules.MaxDate is not null))
            errors.Add(DatesNotForTypeMessage);

        if (
            type != FieldType.MultiChoice
            && (rules.MinSelected is not null || rules.MaxSelected is not null)
        )
            errors.Add(SelectionNotForTypeMessage);
    }

    private static void CheckRanges(FieldRules rules, List<string> errors)
    {
        if (rules.MinLength < 0 || rules.MaxLength < 1)
            errors.Add(NegativeLengthMessage);
        else if (rules.MinLength > rules.MaxLength)
            errors.Add(LengthRangeMessage);

        if (rules.Min > rules.Max)
            errors.Add(NumberRangeMessage);

        if (rules.Decimals is < 0 or > MaxDecimals)
            errors.Add(DecimalsRangeMessage);

        if (rules.MinDate > rules.MaxDate)
            errors.Add(DateRangeMessage);

        if (rules.MinSelected < 0 || rules.MaxSelected < 1)
            errors.Add(NegativeSelectionMessage);
        else if (rules.MinSelected > rules.MaxSelected)
            errors.Add(SelectionRangeMessage);
    }

    private static void CheckOptions(
        FieldType type,
        FieldRules rules,
        IReadOnlyList<ChoiceOption> options,
        List<string> errors
    )
    {
        if (!type.HasOptions)
            return;

        if (options.Count > MaxOptions)
            errors.Add(TooManyOptionsMessage);

        if (options.Any(option => option.Value.Length == 0 || option.Label.Length == 0))
            errors.Add(EmptyOptionMessage);

        if (
            options.Any(option =>
                option.Value.Length > MaxOptionLength || option.Label.Length > MaxOptionLength
            )
        )
            errors.Add(OptionTooLongMessage);

        if (options.GroupBy(option => option.Value).Any(group => group.Count() > 1))
            errors.Add(DuplicateOptionMessage);

        if (options.Count > 0 && rules.MinSelected > options.Count)
            errors.Add(SelectionAboveOptionsMessage);
    }
}
