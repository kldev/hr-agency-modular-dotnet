using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Workers.Application.WorkerDocuments;

/// <summary>
/// The validation both attaching and re-describing a document go through, in one place so the two
/// cannot disagree about what a document may say.
/// </summary>
internal static class WorkerDocumentFactory
{
    public const string FileNameRequiredMessage = "The file name is required.";
    public const string ValidUntilBeforeDocumentDateMessage =
        "A document cannot expire before it was issued.";

    private const int FileNameMaxLength = 255;
    public const string FileNameMaxLengthMessage = "The file name cannot exceed 255 characters.";

    public static (string fileName, string? note) Create(
        string fileName,
        DateOnly documentDate,
        DateOnly? validUntil,
        string? note
    )
    {
        var errors = new List<string>();

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        var name = (fileName ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(FileNameRequiredMessage);

        if (name.Length > FileNameMaxLength)
            errors.Add(FileNameMaxLengthMessage);

        // Optional on purpose: a diploma does not expire, a residence card does.
        if (validUntil is not null && validUntil < documentDate)
            errors.Add(ValidUntilBeforeDocumentDateMessage);

        var (shortNote, noteError) = ShortNote.TryCreate(note ?? "", false);
        if (noteError is not null)
            errors.Add(noteError);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return (name, string.IsNullOrWhiteSpace(shortNote?.Value) ? null : shortNote.Value);
    }
}
