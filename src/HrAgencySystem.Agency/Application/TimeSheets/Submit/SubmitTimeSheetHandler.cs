using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.TimeSheets.Submit;

public static class SubmitTimeSheetHandler
{
    public const string NotYoursMessage =
        "A sheet is sent for approval by the person it belongs to.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(TimeSheetSubmitted, Wolverine.Marten.Events)> Handle(
        SubmitTimeSheet command,
        TimeSheet aggregate,
        IAgencyService service,
        IClock clock,
        CancellationToken ct
    )
    {
        TimeSheetRules.EnsureExists(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        /*
         * Nobody sends somebody else's month. Filling hours in on another person's behalf is a
         * separate question with its own answer - it would have to record who typed them, or
         * "I approve my own hours" stops meaning anything - and it is deliberately not here yet.
         */
        if (command.SubmittedBy != aggregate.UserId)
            throw new BusinessRuleException(NotYoursMessage);

        TimeSheetRules.EnsureCanChange(aggregate.Status, TimeSheetStatus.Submitted);

        if (aggregate.TotalMinutes == 0)
            throw new BusinessRuleException(TimeSheetRules.EmptySheetMessage);

        var submittedBy = await service.GetUserAsync(command.SubmittedBy, ct);

        var submitted = new TimeSheetSubmitted(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            aggregate.TotalMinutes,
            submittedBy,
            clock.UtcNow
        );

        // Same shape as approving with a note: one fact, and a second one only if there was
        // something to say. The role is Owner because only the owner reaches this handler at all.
        if (string.IsNullOrWhiteSpace(command.Comment))
            return (submitted, [submitted]);

        var (note, error) = ShortNote.TryCreate(command.Comment);

        if (error is not null)
            throw new ValidationException(error);

        var commented = new TimeSheetCommented(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            new TimeSheetComment(submittedBy, TimeSheetRole.Owner, note!.Value, clock.UtcNow)
        );

        return (submitted, [submitted, commented]);
    }
}
