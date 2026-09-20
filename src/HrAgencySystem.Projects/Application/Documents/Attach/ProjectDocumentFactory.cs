using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Application.Documents;

internal static class ProjectDocumentFactory
{
    public const string ValidUntilBeforeDocumentDateMessage =
        "A document cannot stop being valid before it was issued.";
    public const string FileNameRequiredMessage = "File name is required.";

    public static (DateOnly documentDate, DateOnly? validUntil, string? note) Validate(
        DateOnly documentDate,
        DateOnly? validUntil,
        string? note,
        string? fileName
    )
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(fileName))
            errors.Add(FileNameRequiredMessage);

        // Optional on purpose: most documents never expire, and forcing a date would invent one.
        if (validUntil is not null && validUntil < documentDate)
            errors.Add(ValidUntilBeforeDocumentDateMessage);

        var (shortNote, noteError) = ShortNote.TryCreate(note ?? "", false);
        if (noteError is not null)
            errors.Add(noteError);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return (
            documentDate,
            validUntil,
            string.IsNullOrWhiteSpace(shortNote?.Value) ? null : shortNote.Value
        );
    }
}
