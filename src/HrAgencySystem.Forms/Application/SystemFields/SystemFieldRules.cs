using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;

namespace HrAgencySystem.Forms.Application.SystemFields;

/// <summary>What a system field's editable part must satisfy, whether it is being defined or changed.</summary>
public static class SystemFieldRules
{
    public const int MaxLabelLength = 300;
    public const int MaxDescriptionLength = 2000;

    public const string LabelRequiredMessage = "A system field needs a label.";
    public const string LabelTooLongMessage = "A label cannot exceed 300 characters.";
    public const string DescriptionTooLongMessage = "A description cannot exceed 2000 characters.";
    public const string SourceTypeMismatchMessage = "This source gives a value of a different type than the field.";
    public const string CodeNotSystemMessage = "A system field's code starts with 'employee.', e.g. employee.pesel.";
    public const string CodeTakenMessage = "The catalogue already has a field with this code.";
    public const string UnknownFieldMessage = "There is no such system field in the catalogue.";
    public const string ArchivedMessage = "This system field is archived.";

    public static List<string> Check(
        FieldType type,
        string label,
        string? description,
        FieldRules rules,
        IReadOnlyList<ChoiceOption> options,
        SystemFieldSource source
    )
    {
        var errors = new List<string>();

        if (label.Length == 0)
            errors.Add(LabelRequiredMessage);
        else if (label.Length > MaxLabelLength)
            errors.Add(LabelTooLongMessage);

        if (description is { Length: > MaxDescriptionLength })
            errors.Add(DescriptionTooLongMessage);

        if (source != SystemFieldSource.None && source.ValueType != type)
            errors.Add(SourceTypeMismatchMessage);

        errors.AddRange(FieldRulesPolicy.Check(type, rules, options));

        return errors;
    }

    public static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
