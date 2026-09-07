namespace HrAgencySystem.Recruitment.Application.JobApplications.Queries;

public interface INoteQueryRepository
{
    Task<IReadOnlyList<ApplicationNoteItem>> GetNotes(Guid organizationId, Guid applicationId, CancellationToken ct);
}

public record ApplicationNoteItem(Guid Id, string Note, string Author, string AuthorEmail, Guid JobApplicationId, DateTimeOffset CreatedAt);
