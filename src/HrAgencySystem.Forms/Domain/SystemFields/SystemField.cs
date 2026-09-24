using HrAgencySystem.Forms.Domain.Layout;

namespace HrAgencySystem.Forms.Domain.SystemFields;

/// <summary>
/// A field defined once for the whole organization: an attribute of the <em>person</em>, shown by
/// any form that asks for it. Five forms asking for a phone number is one value shown five times.
/// <para>
/// <see cref="Code"/> and <see cref="Type"/> never change. Turning <c>employee.pesel</c> from text
/// into a number would invalidate every value already given for it, so that is not an edit - it is
/// a new field, and the old one is archived. The label, the description, the rules and the options
/// may change; a published form version keeps the copy it was published with (plan 028 §3.4).
/// </para>
/// </summary>
public sealed record SystemField(
    Guid SystemFieldId,
    string Code,
    FieldType Type,
    string Label,
    string? Description,
    FieldRules Rules,
    IReadOnlyList<ChoiceOption> Options,
    SystemFieldSource Source,
    bool IsArchived
);
