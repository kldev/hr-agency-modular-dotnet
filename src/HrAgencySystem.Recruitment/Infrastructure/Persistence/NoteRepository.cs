using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
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
}