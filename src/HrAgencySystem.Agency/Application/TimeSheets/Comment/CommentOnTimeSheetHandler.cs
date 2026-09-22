using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.TimeSheets.Comment;

public static class CommentOnTimeSheetHandler
{
    public const string NotOnThisSheetMessage =
        "Only the person this sheet belongs to, somebody above them, or payroll can write on it.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(TimeSheetCommented, Wolverine.Marten.Events)> Handle(
        CommentOnTimeSheet command,
        TimeSheet aggregate,
        IAgencyService service,
        IOrgStructureQueryRepository chart,
        IClock clock,
        CancellationToken ct
    )
    {
        TimeSheetRules.EnsureExists(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var (content, error) = ShortNote.TryCreate(command.Content);

        if (error is not null)
            throw new ValidationException(error);

        var role = await RoleOf(chart, aggregate, command, ct);

        var author = await service.GetUserAsync(command.AuthorId, ct);

        var @event = new TimeSheetCommented(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            aggregate.Year,
            aggregate.Month,
            new TimeSheetComment(author, role, content!.Value, clock.UtcNow)
        );

        return (@event, [@event]);
    }

    /// <summary>
    /// Which hat the author is wearing, frozen onto the comment. "Sent back by the supervisor" has
    /// to keep meaning that after the chart changes.
    /// </summary>
    private static async Task<TimeSheetRole> RoleOf(
        IOrgStructureQueryRepository chart,
        TimeSheet aggregate,
        CommentOnTimeSheet command,
        CancellationToken ct
    )
    {
        if (command.AuthorId == aggregate.UserId)
            return TimeSheetRole.Owner;

        if (command.ActingAsPayroll)
            return TimeSheetRole.Payroll;

        var structure = await chart.GetStructureAsync(aggregate.OrganizationId, ct);

        if (
            structure is not null
            && SupervisorPolicy.IsAbove(structure.AsUnits(), command.AuthorId, aggregate.UserId)
        )
            return TimeSheetRole.Supervisor;

        throw new BusinessRuleException(NotOnThisSheetMessage);
    }
}
