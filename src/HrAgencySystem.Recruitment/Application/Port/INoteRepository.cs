using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface INoteRepository
{
    Task CreateNoteAsync(CreateNoteDocument noteDocument, UserSnapshot user);
    Task<IReadOnlyList<ApplicationNoteItem>> GetNotes(Guid organizationId, Guid applicationId, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public record CreateNoteDocument(Guid JobApplicationId, Guid OrganizationId, Guid CandidateId, ShortNote Text);
public record ApplicationNoteItem(Guid Id, string Note, string Author, string AuthorEmail, Guid JobApplicationId, DateTimeOffset CreatedAt);