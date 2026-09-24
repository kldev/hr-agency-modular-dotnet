using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Domain.ValueObjects;

namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// What is wrong with one element of a layout. <see cref="Target"/> is the id of the page or field
/// the builder should mark, or null for the form as a whole.
/// </summary>
public sealed record LayoutError(Guid? Target, string Label, string Message)
{
    public string ToMessage() => string.IsNullOrWhiteSpace(Label) ? Message : $"{Label}: {Message}";
}

/// <summary>
/// The rules of a form's layout, in two strengths.
/// <para>
/// <see cref="PrepareDraft"/> - is it well formed: unique codes, known system fields, rules that fit
/// their types. A draft may be unfinished (a page with nothing on it yet, a choice with no options
/// yet), because it is saved many times while it is being built.
/// </para>
/// <para>
/// <see cref="ValidatePublish"/> - is it complete enough for somebody to fill in. Only a published
/// version is ever filled, so this is the bar people actually meet.
/// </para>
/// <para>
/// The whole layout is judged at once, which is why the builder saves it whole (plan 028 §3.3):
/// "two fields share a code" is a fact about the set, not about either field.
/// </para>
/// </summary>
public static class FormLayoutPolicy
{
    public const int MaxPages = 30;
    public const int MaxFields = 300;
    public const int MaxTitleLength = 200;
    public const int MaxLabelLength = 300;
    public const int MaxDescriptionLength = 2000;
    public const int MaxPlaceholderLength = 200;

    public const string TooManyPagesMessage = "A form cannot have more than 30 pages.";
    public const string TooManyFieldsMessage = "A form cannot have more than 300 fields.";
    public const string DuplicatePageMessage = "Two pages share the same id.";
    public const string DuplicateFieldIdMessage = "Two fields share the same id.";
    public const string PageTitleRequiredMessage = "A page needs a title.";
    public const string PageTitleTooLongMessage = "A page title cannot exceed 200 characters.";
    public const string LabelRequiredMessage = "A field needs a label.";
    public const string LabelTooLongMessage = "A label cannot exceed 300 characters.";
    public const string DescriptionTooLongMessage = "A description cannot exceed 2000 characters.";
    public const string PlaceholderTooLongMessage = "A placeholder cannot exceed 200 characters.";
    public const string DuplicateCodeMessage = "Another field on this form already uses this code.";
    public const string SystemFieldRequiredMessage = "Pick the system field this one shows.";
    public const string UnknownSystemFieldMessage = "There is no such system field in the catalogue.";
    public const string ArchivedSystemFieldMessage = "This system field is archived. Remove it from the form.";
    public const string VisibilityNotSupportedMessage = "Conditional fields are not supported yet.";
    public const string NoPagesMessage = "A form needs at least one page before it can be published.";
    public const string EmptyPageMessage = "Every page needs at least one field before the form can be published.";
    public const string NoOptionsMessage = "A choice field needs at least one option before the form can be published.";

    /// <summary>
    /// Cleans the layout the builder sent, fills every system field from the catalogue and says what
    /// is wrong with the result. The returned pages are what gets stored, errors or not; the caller
    /// stores nothing when there are errors.
    /// </summary>
    public static (IReadOnlyList<FormPage> pages, IReadOnlyList<LayoutError> errors) PrepareDraft(
        IReadOnlyList<FormPage>? input,
        IReadOnlyList<SystemField> catalogue
    )
    {
        var errors = new List<LayoutError>();
        var pages = new List<FormPage>();

        foreach (var page in input ?? [])
        {
            var fields = (page.Fields ?? [])
                .Select(field => Prepare(field, catalogue, errors))
                .ToList();

            pages.Add(page with
            {
                Title = (page.Title ?? "").Trim(),
                Description = Blank(page.Description),
                Fields = fields,
            });
        }

        CheckShape(pages, errors);

        return (pages, errors);
    }

    public static IReadOnlyList<LayoutError> ValidatePublish(IReadOnlyList<FormPage> pages)
    {
        var errors = new List<LayoutError>();

        if (pages.Count == 0)
            errors.Add(new LayoutError(null, "", NoPagesMessage));

        foreach (var page in pages.Where(page => page.Fields.Count == 0))
            errors.Add(new LayoutError(page.PageId, page.Title, EmptyPageMessage));

        foreach (var field in pages.AllFields.Where(field => field.Type.HasOptions && field.Options.Count == 0))
            errors.Add(new LayoutError(field.FieldId, field.Label, NoOptionsMessage));

        return errors;
    }

    private static FormField Prepare(FormField field, IReadOnlyList<SystemField> catalogue, List<LayoutError> errors)
    {
        var prepared = field.Source == FieldSource.System
            ? SystemFieldResolver.Resolve(field, catalogue, errors)
            : PrepareOwn(field, errors);

        var label = prepared.Label;

        if (field.VisibleWhen is not null)
            errors.Add(new LayoutError(field.FieldId, label, VisibilityNotSupportedMessage));

        if (prepared.Description is { Length: > MaxDescriptionLength })
            errors.Add(new LayoutError(field.FieldId, label, DescriptionTooLongMessage));

        if (prepared.Placeholder is { Length: > MaxPlaceholderLength })
            errors.Add(new LayoutError(field.FieldId, label, PlaceholderTooLongMessage));

        return prepared;
    }

    private static FormField PrepareOwn(FormField field, List<LayoutError> errors)
    {
        var label = (field.Label ?? "").Trim();
        var (rules, options) = FieldRulesPolicy.Normalize(field.Type, field.Rules, field.Options);

        var prepared = field with
        {
            SystemFieldId = null,
            Code = (field.Code ?? "").Trim(),
            Label = label,
            LabelOverride = null,
            Description = Blank(field.Description),
            Placeholder = Blank(field.Placeholder),
            Rules = rules,
            Options = options,
        };

        var (code, codeError) = FieldCode.TryCreate(prepared.Code);

        if (codeError is not null)
            errors.Add(new LayoutError(field.FieldId, label, codeError));
        else if (code!.IsSystemNamespace)
            errors.Add(new LayoutError(field.FieldId, label, FieldCode.ReservedPrefixMessage));

        if (label.Length == 0)
            errors.Add(new LayoutError(field.FieldId, label, LabelRequiredMessage));
        else if (label.Length > MaxLabelLength)
            errors.Add(new LayoutError(field.FieldId, label, LabelTooLongMessage));

        foreach (var error in FieldRulesPolicy.Check(prepared.Type, rules, options))
            errors.Add(new LayoutError(field.FieldId, label, error));

        if (prepared.DefaultValue is { IsEmpty: true })
            prepared = prepared with { DefaultValue = null };

        if (prepared.DefaultValue is not null
            && FormAnswersValidator.ValidateValue(prepared, prepared.DefaultValue, ValidationMode.Draft) is { } invalid)
            errors.Add(new LayoutError(field.FieldId, label, $"Default value: {invalid.Message}"));

        return prepared;
    }

    private static void CheckShape(List<FormPage> pages, List<LayoutError> errors)
    {
        if (pages.Count > MaxPages)
            errors.Add(new LayoutError(null, "", TooManyPagesMessage));

        var fields = pages.SelectMany(page => page.Fields).ToList();

        if (fields.Count > MaxFields)
            errors.Add(new LayoutError(null, "", TooManyFieldsMessage));

        if (pages.GroupBy(page => page.PageId).Any(group => group.Count() > 1))
            errors.Add(new LayoutError(null, "", DuplicatePageMessage));

        if (fields.GroupBy(field => field.FieldId).Any(group => group.Count() > 1))
            errors.Add(new LayoutError(null, "", DuplicateFieldIdMessage));

        foreach (var page in pages)
        {
            if (page.Title.Length == 0)
                errors.Add(new LayoutError(page.PageId, "", PageTitleRequiredMessage));
            else if (page.Title.Length > MaxTitleLength)
                errors.Add(new LayoutError(page.PageId, page.Title, PageTitleTooLongMessage));
        }

        // Every holder of a repeated code is marked, not just the second one - the builder cannot
        // know which of the two the author meant to rename.
        foreach (var field in fields.Where(field => field.Code.Length > 0)
                     .GroupBy(field => field.Code)
                     .Where(group => group.Count() > 1)
                     .SelectMany(group => group))
            errors.Add(new LayoutError(field.FieldId, field.Label, DuplicateCodeMessage));
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
