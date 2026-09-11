using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Interviews.ChangeFormat;

public static class ChangeInterviewFormatHandler
{
    [AggregateHandler]
    public static async Task<(InterviewFormatChanged, Wolverine.Marten.Events)> Handle(
        ChangeInterviewFormat command,
        Interview aggregate,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new InterviewFormatChanged(
            command.InterviewId, aggregate.Format, command.Format, user, clock.UtcNow);

        return (@event, [@event]);
    }
}