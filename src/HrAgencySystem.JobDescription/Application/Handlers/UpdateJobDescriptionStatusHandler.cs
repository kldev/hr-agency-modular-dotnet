using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Application.Result;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.JobDescription.Application.Handlers;

public static class UpdateJobDescriptionStatusHandler
{
    [AggregateHandler]
    public static async Task<(UpdateJobDescriptionStatusResult, Wolverine.Marten.Events)> Handle(
        UpdateJobDescriptionStatus command,
        Domain.JobDescription aggregate,
        IUserSnapshotService snapshotService,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Not found " + command.JobDescriptionId);
        var result = new UpdateJobDescriptionStatusResult(aggregate.Id.Value, command.Status);

        var modifiedBy = await snapshotService.GetUserAsync(command.ModifiedBy, ct);
        if (modifiedBy == null)
            throw new BusinessRuleException(IUserSnapshotService.NotFoundMessage);
        
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
            default:
                throw new BusinessRuleException("Invalid status change: " + command.Status);
        }
    }
}