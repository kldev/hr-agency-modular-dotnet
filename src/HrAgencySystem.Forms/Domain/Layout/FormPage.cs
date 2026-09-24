namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// One step. A form with one page is a plain form and a form with several is a wizard - the same
/// model either way, so there is no "simple form" special case anywhere.
/// </summary>
public sealed record FormPage(
    Guid PageId,
    string Title,
    string? Description,
    IReadOnlyList<FormField> Fields
);

public static class FormPages
{
    extension(IReadOnlyList<FormPage> pages)
    {
        public IEnumerable<FormField> AllFields => pages.SelectMany(page => page.Fields);

        public FormField? FieldByCode(string code) =>
            pages.AllFields.FirstOrDefault(field => field.Code == code);
    }
}
