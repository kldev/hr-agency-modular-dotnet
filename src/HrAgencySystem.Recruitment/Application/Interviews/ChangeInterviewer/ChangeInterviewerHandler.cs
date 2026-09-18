using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Interviews.ChangeInterviewer;

public static class ChangeInterviewerHandler
{
    [AggregateHandler]
    public static async Task<(InterviewerChanged, Wolverine.Marten.Events)> Handle(
        ChangeInterviewer command,
        Interview aggregate,
        IDocumentSession session,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var interviewer = await service.GetUserAsync(command.InterviewerId, ct);

        var @event = new InterviewerChanged(
            command.InterviewId,
            aggregate.Interviewer,
            interviewer,
            user,
            clock.UtcNow
        );

        if (string.IsNullOrEmpty(command.Note))
            return (@event, [@event]);

        await service.AppendApplicationNoteToStream(
            aggregate.JobApplicationId,
            OrganizationId.From(command.OrganizationId),
            command.Note,
            user,
            ct
        );

        return (@event, [@event]);
    }
}
