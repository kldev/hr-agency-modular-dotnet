using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public class NoteRepository(IDocumentSession session, IUserSnapshotRepository snapshotRepository, IClock clock) : INoteRepository
{
    public async Task CreateNoteAsync(CreateNote note, CancellationToken ct)
    {
        var user = await GetCreatedBy(snapshotRepository, note.AuthorId, ct);
        
        var noteDocument = JobApplicationNote.Create(note.JobApplicationId,
            note.OrganizationId,
            note.CandidateId, note.Text,
            user.Id, user, clock.UtcNow);
        
        session.Insert(noteDocument);
    }

    public async Task<IReadOnlyList<ApplicationNoteItem>> GetNotes(Guid organizationId, Guid applicationId, CancellationToken ct)
    {
        var result = await session.Query<JobApplicationNote>()
            .Where(z => z.OrgId == organizationId)
            .Where(z => z.JobApplicationId == applicationId)
            .Where(z => z.IsDeleted == false)
            .OrderByDescending(z=>z.CreatedAt)
            .Select(z => new ApplicationNoteItem(z.Id, z.Note, z.CreatedBy.Fullname, z.CreatedBy.Email, z.CreatedAt))
            .ToListAsync<ApplicationNoteItem>(ct);

        return result;
        // return [.. result.OrderByDescending(z => z.CreatedAt)];
    }

    private static async Task<UserSnapshot> GetCreatedBy(IUserSnapshotRepository repository, Guid createById,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(createById, ct);
        return user ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}