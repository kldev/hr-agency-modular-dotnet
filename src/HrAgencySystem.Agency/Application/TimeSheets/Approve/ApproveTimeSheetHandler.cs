using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.TimeSheets.Approve;

/// <summary>
/// The supervisor accepts the month. Who that is comes from the chart at this moment, not from
/// anything written on the sheet - after a reorganisation the month should be closed by whoever
/// answers for the person now.
/// </summary>
public static class ApproveTimeSheetHandler
{
    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(TimeSheetApproved, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        ApproveTimeSheet command,
        TimeSheet aggregate,
        IAgencyService service,
        IOrgStructureQueryRepository chart,
        IClock clock,
        CancellationToken ct
    )
    {
        TimeSheetRules.EnsureExists(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        TimeSheetRules.EnsureCanChange(aggregate.Status, TimeSheetStatus.Approved);

        await TimeSheetRules.EnsureIsSupervisorOf(
            chart,
            aggregate.OrganizationId,
            command.ApprovedBy,
            aggregate.UserId,
            ct
        );

        var approvedBy = await service.GetUserAsync(command.ApprovedBy, ct);
        var owner = await service.GetUserAsync(aggregate.UserId, ct);

        var approved = new TimeSheetApproved(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            approvedBy,
            clock.UtcNow
        );

        // A note on approval is optional - unlike a return, where saying nothing would invite a
        // second return.
        if (string.IsNullOrWhiteSpace(command.Comment))
            return (approved, [approved], Mail(aggregate, owner, approvedBy, ""));

        var (note, error) = ShortNote.TryCreate(command.Comment);

        if (error is not null)
            throw new ValidationException(error);

        var commented = new TimeSheetCommented(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            new TimeSheetComment(approvedBy, TimeSheetRole.Supervisor, note!.Value, clock.UtcNow)
        );

        return (approved, [approved, commented], Mail(aggregate, owner, approvedBy, note.Value));
    }

    /// <summary>
    /// Nothing goes out when the decision was the owner's own. Belt and braces here, unlike on a
    /// return or a settlement: the supervisor check already makes approving your own month
    /// impossible, because walking up the chart never finds the person who asked. One comparison is
    /// cheap enough to keep that true if the policy ever changes.
    /// </summary>
    private static OutgoingMessages Mail(
        TimeSheet aggregate,
        UserSnapshot owner,
        UserSnapshot approvedBy,
        string comment
    )
    {
        var messages = new OutgoingMessages();

        if (approvedBy.Id == owner.Id)
            return messages;

        messages.Add(
            new SendTimeSheetApproved(
                Guid.NewGuid(),
                nameof(ApproveTimeSheetHandler),
                owner.Email,
                owner.Fullname,
                aggregate.Year,
                aggregate.Month,
                TimeSheetPeriodLabel.For(aggregate.Year, aggregate.Month),
                approvedBy.Fullname,
                comment
            )
        );

        return messages;
    }
}
