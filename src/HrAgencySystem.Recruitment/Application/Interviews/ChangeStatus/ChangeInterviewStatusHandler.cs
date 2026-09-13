using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Interviews.ChangeStatus;

public static class ChangeInterviewStatusHandler
{
    [AggregateHandler]
    public static async Task<(InterviewStatusChanged, Wolverine.Marten.Events)> Handle(
        ChangeInterviewStatus command,
        Interview aggregate,
        IRecruitmentService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new InterviewStatusChanged(
            command.InterviewId, aggregate.Status, command.Status, user, clock.UtcNow);
        
        
        if (string.IsNullOrEmpty(command.Note)) return (@event, [@event]);
        
        await service.AppendApplicationNoteToStream(aggregate.JobApplicationId,
            OrganizationId.From(command.OrganizationId), command.Note, user, ct);

        return (@event, [@event]);
    }
}