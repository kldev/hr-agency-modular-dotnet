using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.TimeSheets.RemoveWorkDay;

public static class RemoveWorkDayHandler
{
    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(WorkDayRemoved, Wolverine.Marten.Events)> Handle(
        RemoveWorkDay command,
        TimeSheet aggregate,
        IAgencyService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        TimeSheetRules.EnsureEditable(aggregate);

        if (aggregate.DayOn(command.Date) is null)
            throw new BusinessRuleException(TimeSheetRules.NoDayOnThatDateMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new WorkDayRemoved(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            command.Date,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
