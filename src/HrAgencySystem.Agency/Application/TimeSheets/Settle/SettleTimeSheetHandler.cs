using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.TimeSheets.Settle;

/// <summary>
/// Hands an approved month to payroll. There are no amounts here and that is the whole design:
/// settling means the hours are agreed and passed on, not that a payment was worked out. Money
/// would drag in the hourly minimum, contributions and corrections - that is bookkeeping.
/// </summary>
public static class SettleTimeSheetHandler
{
    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(TimeSheetSettled, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        SettleTimeSheet command,
        TimeSheet aggregate,
        IAgencyService service,
        IClock clock,
        CancellationToken ct
    )
    {
        TimeSheetRules.EnsureExists(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        // The endpoint already refuses anybody else; checked again here so the rule survives a
        // second caller and can be tested without a request.
        if (!command.ActingAsPayroll)
            throw new BusinessRuleException(TimeSheetRules.NotPayrollMessage);

        TimeSheetRules.EnsureCanChange(aggregate.Status, TimeSheetStatus.Settled);

        var settledBy = await service.GetUserAsync(command.SettledBy, ct);
        var owner = await service.GetUserAsync(aggregate.UserId, ct);

        var @event = new TimeSheetSettled(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            settledBy,
            clock.UtcNow
        );

        var messages = new OutgoingMessages();

        // Payroll settles everybody, itself included, so this is the second place the rule earns
        // its keep rather than merely stating itself.
        if (settledBy.Id != owner.Id)
        {
            messages.Add(
                new SendTimeSheetSettled(
                    Guid.NewGuid(),
                    nameof(SettleTimeSheetHandler),
                    owner.Email,
                    owner.Fullname,
                    aggregate.Year,
                    aggregate.Month,
                    TimeSheetPeriodLabel.For(aggregate.Year, aggregate.Month),
                    settledBy.Fullname
                )
            );
        }

        return (@event, [@event], messages);
    }
}
