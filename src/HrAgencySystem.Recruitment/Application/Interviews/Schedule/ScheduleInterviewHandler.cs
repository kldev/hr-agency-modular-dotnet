using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Recruitment.Application.Interviews.Schedule;

// ReSharper disable once UnusedType.Global
public static class ScheduleInterviewHandler
{
    public static async Task<(InterviewCreated, Wolverine.Marten.Events)> Handle(
        ScheduleInterview command,
        IOrganizationChecker checker,
        IJobApplicationInfoQueryRepository queryRepository,
        IUserSnapshotRepository userSnapshotRepository,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {

        var user = await GetUser(userSnapshotRepository, command.CreatedBy, ct);
        var interviewer = await GetUser(userSnapshotRepository, command.InterviewerId, ct);
        var organizationId = OrganizationId.From(command.OrganizationId);
        var application = await GetApplication(queryRepository, command.JobApplicationId,
            command.OrganizationId, ct);
        
        await ValidateOrganization(checker, command.OrganizationId, ct);

        var interviewId = InterviewId.New();

        var (shortNote, error) = ShortNote.TryCreate(command.Note, false);
        if (error != null) throw new ValidationException(error);

        var @event = new InterviewCreated(
            interviewId.Value,
            organizationId.Value,
            command.JobApplicationId,
            application.CandidateId,
            application.CompanyId,
            command.ScheduledAtInstant,
            command.ScheduledTimezone,
            interviewer,
            command.Format,
            command.InterviewType,
            shortNote!.Value,
            user,
            clock.UtcNow
        );

        var jobApplicationEvent = new JobApplicationInterviewScheduled(command.JobApplicationId,
            clock.UtcNow, user, interviewId.Value
        );

        session.Events.StartStream<Interview>(interviewId.Value, @event);

        return (@event, [@event, jobApplicationEvent]);
    }
    
    private static async Task<JobApplicationInfo> GetApplication(IJobApplicationInfoQueryRepository repository, Guid jobApplicationId, Guid organizationId,
        CancellationToken ct)
    {
        var application = await repository.GetAsync(organizationId,  jobApplicationId, ct);
        return application ?? throw new NotFoundException("Job application", jobApplicationId);
    }
    
    private static async Task ValidateOrganization(IOrganizationChecker checker, Guid organizationId,
        CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(organizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }
    
    private static async Task<UserSnapshot> GetUser(IUserSnapshotRepository repository, Guid userId,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(userId, ct);
        return user ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}