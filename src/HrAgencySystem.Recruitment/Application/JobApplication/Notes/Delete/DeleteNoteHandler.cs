using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Notes.Delete;


// ReSharper disable once UnusedType.Global
public static class DeleteNoteHandler
{
    public static async Task<JobApplicationNoteDeleted> Handle(DeleteNote command,
        IUserSnapshotRepository userRepository,
        IJobApplicationQueryRepository queryRepository,
        IClock clock,
        IDocumentSession session,
        CancellationToken ct)
    {
        var user = await GetUser(userRepository, command.RemovedBy, ct);
        var application = await GetApplication(queryRepository, command.JobApplicationId, command.OrganizationId, ct);

        var document = await GetNoteDocument(command, session, ct);

        var deletedDocument = document!.Delete(user, clock.UtcNow);
        
        session.Update(deletedDocument);

        var @event = new JobApplicationNoteDeleted(application.Id, application.CandidateId, user, clock.UtcNow);
        return @event;
    }

    private static async Task<JobApplicationNote?> GetNoteDocument(DeleteNote command, IDocumentSession session, CancellationToken ct)
    {
        var document = await session.Query<JobApplicationNote>()
            .Where(z => z.Id == command.NoteId && z.JobApplicationId == command.JobApplicationId)
            .FirstOrDefaultAsync(ct);

        return document ?? throw new NotFoundException("Job application note", command.NoteId);
    }

    private static async Task<JobApplicationProjection> GetApplication(IJobApplicationQueryRepository repository, Guid jobApplicationId, Guid organizationId,
        CancellationToken ct)
    {
        var application = await repository.GetJobApplication(organizationId,  jobApplicationId, ct);
        return application ?? throw new NotFoundException("Job application", jobApplicationId);
    }

    
    private static async Task<UserSnapshot> GetUser(IUserSnapshotRepository repository, Guid userId,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(userId, ct);
        return user ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}