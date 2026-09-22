using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.TimeSheets.Return;

/// <summary>
/// Sends the month back to be filled in again. Reachable from two places on purpose: the supervisor
/// sends back what they were asked to approve, and payroll sends back what it has already been
/// given. There is no "rejected" - hours are not refused, they are corrected.
/// </summary>
public static class ReturnTimeSheetForCorrectionHandler
{
    public const string ReasonRequiredMessage =
        "Say what needs correcting. A sheet handed back without a reason comes straight back.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(TimeSheetReturnedForCorrection, Wolverine.Marten.Events)> Handle(
        ReturnTimeSheetForCorrection command,
        TimeSheet aggregate,
        IAgencyService service,
        IOrgStructureQueryRepository chart,
        IClock clock,
        CancellationToken ct
    )
    {
        TimeSheetRules.EnsureExists(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        TimeSheetRules.EnsureCanChange(aggregate.Status, TimeSheetStatus.Correction);

        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new ValidationException(ReasonRequiredMessage);

        var (reason, error) = ShortNote.TryCreate(command.Reason);

        if (error is not null)
            throw new ValidationException(error);

        // Payroll may send back what it has been handed; anybody else has to be above the person.
        if (!command.ActingAsPayroll)
            await TimeSheetRules.EnsureIsSupervisorOf(
                chart,
                aggregate.OrganizationId,
                command.ReturnedBy,
                aggregate.UserId,
                ct
            );

        var returnedBy = await service.GetUserAsync(command.ReturnedBy, ct);

        var @event = new TimeSheetReturnedForCorrection(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            new TimeSheetComment(
                returnedBy,
                command.ActingAsPayroll ? TimeSheetRole.Payroll : TimeSheetRole.Supervisor,
                reason!.Value,
                clock.UtcNow
            ),
            returnedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
