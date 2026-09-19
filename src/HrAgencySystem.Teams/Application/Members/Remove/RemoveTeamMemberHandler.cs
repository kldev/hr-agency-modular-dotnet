using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Teams.Application.Members.Remove;

public static class RemoveTeamMemberHandler
{
    public const string LastMemberMessage =
        "The last member cannot be removed — a team without anybody on it handles nothing.";

    [AggregateHandler]
    public static async Task<(TeamMemberRemoved, Wolverine.Marten.Events)> Handle(
        RemoveTeamMember command,
        Team aggregate,
        ITeamsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var current =
            aggregate.FindMember(command.UserId)
            ?? throw new NotFoundException("Team member", command.UserId);

        if (aggregate.Members.Count <= 1)
            throw new BusinessRuleException(LastMemberMessage);

        var removed = await service.GetUserAsync(command.UserId, ct);
        var removedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        // No mail: in this system a notification goes to whoever gains a responsibility, never to
        // whoever loses one.
        var @event = new TeamMemberRemoved(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            new TeamMemberSnapshot(removed, current.Role),
            removedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
