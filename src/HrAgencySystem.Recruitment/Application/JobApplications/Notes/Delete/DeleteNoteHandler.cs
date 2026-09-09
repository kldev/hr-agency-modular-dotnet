using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Notes.Delete;


// ReSharper disable once UnusedType.Global
public static class DeleteNoteHandler
{
    public static async Task<JobApplicationNoteDeleted> Handle(
        DeleteNote command,
        IRecruitmentService service,
        IClock clock,
        IDocumentSession session,
        CancellationToken ct)
    {
        var user = await service.GetUserAsync(command.RemovedBy, ct);
        var application = await service.GetApplicationAsync(command.JobApplicationId, command.OrganizationId, ct);

        var document = await GetNoteDocument(command, session, ct);

        var deletedDocument = document!.Delete(user, clock.UtcNow);
        
        session.Update(deletedDocument);

        var @event = new JobApplicationNoteDeleted(
            application.JobApplicationId, 
            application.CandidateId,
            user, 
            clock.UtcNow);
        return @event;
    }

    private static async Task<JobApplicationNote?> GetNoteDocument(DeleteNote command, IDocumentSession session, CancellationToken ct)
    {
        var document = await session.Query<JobApplicationNote>()
            .Where(z => z.Id == command.NoteId && z.JobApplicationId == command.JobApplicationId)
            .FirstOrDefaultAsync(ct);

        return document ?? throw new NotFoundException("Job application note", command.NoteId);
    }
}