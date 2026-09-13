using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Interviews.Reschedule;

public static class RescheduleInterviewHandler
{
    [AggregateHandler]
    public static async Task<(InterviewRescheduled, Wolverine.Marten.Events)> Handle(
        RescheduleInterview command,
        Interview aggregate,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var (shortNote, error) = ShortNote.TryCreate(command.Note ?? "", false);
        if (error != null) throw new ValidationException(error);

        var @event = new InterviewRescheduled(
            command.InterviewId,
            command.OrganizationId,
            command.ScheduledAtInstant,
            command.ScheduledTimezone,
            shortNote!.Value,
            user,
            clock.UtcNow
        );

        if (string.IsNullOrEmpty(command.Note)) return (@event, [@event]);

        await service.AppendApplicationNoteToStream(aggregate.JobApplicationId,
            OrganizationId.From(command.OrganizationId), command.Note, user, ct);

        return (@event, [@event]);
    }
}