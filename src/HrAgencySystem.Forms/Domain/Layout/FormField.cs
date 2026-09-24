using HrAgencySystem.Forms.Domain.Values;

namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// One field on a page.
/// <para>
/// A <see cref="FieldSource.System"/> field carries a copy of its catalogue definition - code, type,
/// rules, options, description - refreshed from the catalogue whenever the draft is saved and frozen
/// for good when a version is published (see <see cref="SystemFields.SystemFieldResolver"/>). The
/// form may only rename it: <see cref="LabelOverride"/> wins over the catalogue label, and
/// <see cref="Label"/> is always the label to show.
/// </para>
/// <para>
/// A <see cref="FieldSource.Form"/> field is entirely the form's own; <see cref="LabelOverride"/>
/// stays null.
/// </para>
/// </summary>
public sealed record FormField(
    Guid FieldId,
    FieldSource Source,
    Guid? SystemFieldId,
    string Code,
    FieldType Type,
    string Label,
    string? LabelOverride,
    string? Description,
    string? Placeholder,
    FieldRules Rules,
    IReadOnlyList<ChoiceOption> Options,
    FieldValue? DefaultValue,
    FieldVisibility? VisibleWhen
);
