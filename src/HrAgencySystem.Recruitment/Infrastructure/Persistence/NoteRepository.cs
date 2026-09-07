using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public class NoteRepository(IDocumentSession session, IClock clock) : INoteRepository
{
    public Task CreateNoteAsync(CreateNoteDocument note, UserSnapshot user)
    {
        var noteDocument = JobApplicationNote.Create(note.JobApplicationId,
            note.OrganizationId,
            note.CandidateId, note.Text,
            user, clock.UtcNow);

        session.Insert(noteDocument);

        return Task.CompletedTask;
    }

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