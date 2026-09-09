using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Reactivate;

public static class ReactivateJobApplicationHandler
{

    [AggregateHandler]
    public static async Task<(JobApplicationReactivated, Wolverine.Marten.Events)> Handle(
        ReactivateJobApplication command,
        JobApplication aggregate,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct)
    {
        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        if (aggregate.Status != JobApplicationStatus.Withdrawn)
            throw new BusinessRuleException("Only Withdrawn application can be reactivated");

        var @event = new JobApplicationReactivated(command.JobApplicationId, clock.UtcNow, user);

        return (@event, [@event]);
    }
}