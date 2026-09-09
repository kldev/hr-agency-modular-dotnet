using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.JobDescription.Application.ChangeStatus;

public static class ChangeJobDescriptionStatusHandler
{
    [AggregateHandler]
    public static async Task<(UpdateJobDescriptionStatusResult, Wolverine.Marten.Events)> Handle(
        ChangeJobDescriptionStatus command,
        Domain.JobDescription aggregate,
        IJobDescriptionService service,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Job description", command.JobDescriptionId);
        var result = new UpdateJobDescriptionStatusResult(aggregate.Id.Value, command.Status);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        if (aggregate.Status == command.Status)
        {
            return (result, []);
        }
        
        switch (command.Status)
        {
            case JobDescriptionStatus.Closed:
                var @closedEvent = new JobDescriptionClosed(aggregate.Id.Value, modifiedBy, clock.UtcNow);
                return (result, [@closedEvent]);
            case JobDescriptionStatus.Cancelled:
                var @canceledEvent = new JobDescriptionCancelled(aggregate.Id.Value, modifiedBy, clock.UtcNow);
                return (result, [@canceledEvent]);
            case JobDescriptionStatus.OnHold:
                var @holdEvent = new JobDescriptionPutOnHold(aggregate.Id.Value, modifiedBy, clock.UtcNow);
                return (result, [holdEvent]);
            case JobDescriptionStatus.Open:
                var @openEvent = new JobDescriptionOpened(aggregate.Id.Value, modifiedBy, clock.UtcNow);
                return (result, [@openEvent]);
            case JobDescriptionStatus.Draft:
            default:
                throw new BusinessRuleException("Invalid status change: " + command.Status);
        }
    }
}

public sealed record UpdateJobDescriptionStatusResult(Guid JobDescriptionId, JobDescriptionStatus Status);