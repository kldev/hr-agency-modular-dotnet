using HrAgencySystem.Forms.Domain.Layout;

namespace HrAgencySystem.Forms.Domain.SystemFields;

/// <summary>
/// Fills a system field on a form from the catalogue. Everything that defines the field - code,
/// type, rules, options, description - comes from the catalogue; the form keeps only its id, its
/// own placeholder and, if it wants one, its own label.
/// <para>
/// Run on every draft save and once more at publication, so a draft always shows the catalogue as
/// it is today while a published version keeps the catalogue as it was then. That is the whole of
/// system field versioning: the copy inside the version is the version.
/// </para>
/// </summary>
public static class SystemFieldResolver
{
    public static FormField Resolve(
        FormField field,
        IReadOnlyList<SystemField> catalogue,
        List<LayoutError> errors
    )
    {
        var labelOverride = string.IsNullOrWhiteSpace(field.LabelOverride) ? null : field.LabelOverride.Trim();
        var placeholder = string.IsNullOrWhiteSpace(field.Placeholder) ? null : field.Placeholder.Trim();

        if (field.SystemFieldId is null)
        {
            errors.Add(new LayoutError(field.FieldId, field.Label ?? "", FormLayoutPolicy.SystemFieldRequiredMessage));

            return field;
        }

        var definition = catalogue.FirstOrDefault(candidate => candidate.SystemFieldId == field.SystemFieldId);

        if (definition is null)
        {
            errors.Add(new LayoutError(field.FieldId, field.Label ?? "", FormLayoutPolicy.UnknownSystemFieldMessage));

            return field;
        }

        if (definition.IsArchived)
            errors.Add(new LayoutError(field.FieldId, definition.Label, FormLayoutPolicy.ArchivedSystemFieldMessage));

        if (labelOverride is { Length: > FormLayoutPolicy.MaxLabelLength })
            errors.Add(new LayoutError(field.FieldId, definition.Label, FormLayoutPolicy.LabelTooLongMessage));

        return field with
        {
            Code = definition.Code,
            Type = definition.Type,
            Label = labelOverride ?? definition.Label,
            LabelOverride = labelOverride,
            Description = definition.Description,
            Placeholder = placeholder,
            Rules = definition.Rules,
            Options = definition.Options,
            // A system field starts from what is known about the person, never from a constant.
            DefaultValue = null,
        };
    }
}
