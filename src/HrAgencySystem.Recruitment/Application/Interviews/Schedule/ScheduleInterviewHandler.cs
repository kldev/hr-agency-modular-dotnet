using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
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
        IRecruitmentService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var user = await service.GetUserAsync(command.CreatedBy, ct);
        var interviewer = await service.GetUserAsync(command.InterviewerId, ct);
        var organizationId = OrganizationId.From(command.OrganizationId);
        var application = await service.GetApplicationAsync(command.JobApplicationId,
            command.OrganizationId, ct);
        
        await service.ValidateOrganization(command.OrganizationId, ct);

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
}