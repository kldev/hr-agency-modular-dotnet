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
    
    private static async Task<UserSnapshot> GetCreatedBy(IUserSnapshotRepository repository, Guid createById,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(createById, ct);
        return user ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}