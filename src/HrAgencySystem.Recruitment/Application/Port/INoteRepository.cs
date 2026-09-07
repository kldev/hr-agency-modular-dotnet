using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface INoteRepository
{
    Task CreateNoteAsync(CreateNote note, CancellationToken ct);
    Task<IReadOnlyList<ApplicationNoteItem>> GetNotes(Guid organizationId, Guid applicationId, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public record CreateNote(Guid JobApplicationId, Guid OrganizationId, Guid CandidateId, ShortNote Text, Guid AuthorId);
public record ApplicationNoteItem(Guid Id, string Note, string Author, string AuthorEmail, DateTimeOffset CreatedAt);