using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Documents;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public sealed class NoteQueryRepository(IQuerySession session) : INoteQueryRepository
{
    public async Task<IReadOnlyList<ApplicationNoteItem>> GetNotes(Guid organizationId, Guid applicationId, CancellationToken ct)
    {
        var result = await session.Query<JobApplicationNote>()
            .Where(z => z.OrgId == organizationId)
            .Where(z => z.JobApplicationId == applicationId)
            .Where(z => z.IsDeleted == false)
            .OrderByDescending(z=>z.CreatedAt)
            .Select(z => new ApplicationNoteItem(z.Id, z.Note, z.CreatedBy.Fullname, z.CreatedBy.Email, z.JobApplicationId, z.CreatedAt))
            .ToListAsync<ApplicationNoteItem>(ct);

        return result;
    }
}