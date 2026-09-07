using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Notes.Create;

// ReSharper disable once UnusedType.Global
public static class CreateNoteHandler
{
    public static async Task<JobApplicationNoteAdded> Handle(CreateNote command,
        IUserSnapshotRepository userRepository,
        IClock clock,
        IJobApplicationInfoQueryRepository queryRepository,
        INoteRepository noteRepository,
        CancellationToken ct)
    {
        var user = await GetCreatedBy(userRepository, command.AddedBy, ct);
        var application = await GetApplication(queryRepository, command.JobApplicationId, command.OrganizationId, ct);

        var (shortNote, error) = ShortNote.TryCreate(command.Text);
        if (error != null) throw new ValidationException(error);

        var @event = new JobApplicationNoteAdded(application.JobApplicationId, application.CandidateId, clock.UtcNow,
            shortNote!.Value, user);

        var createNote = new CreateNoteDocument(application.JobApplicationId, application.OrganizationId,
            application.CandidateId,
            shortNote!);

        await noteRepository.CreateNoteAsync(createNote, user);

        return @event;

    }

    private static async Task<JobApplicationInfo> GetApplication(IJobApplicationInfoQueryRepository repository,
        Guid jobApplicationId, Guid organizationId,
        CancellationToken ct)
    {
        var application = await repository.GetAsync(jobApplicationId, OrganizationId.From(organizationId), ct);
        return application ?? throw new NotFoundException("Job application", jobApplicationId);
    }

    private static async Task<UserSnapshot> GetCreatedBy(IUserSnapshotRepository repository, Guid createdById,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(createdById, ct);
        return user ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}