using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Services;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Teams.Application.Members.Remove;

public static class RemoveTeamMemberHandler
{
    public const string LastMemberMessage =
        "The last member cannot be removed — a team without anybody on it handles nothing.";

    [AggregateHandler]
    public static async Task<(TeamMemberRemoved, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        RemoveTeamMember command,
        Team aggregate,
        ITeamsService service,
        ITeamMembershipReservationRepository reservations,
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

        // Releasing here rather than on the projection: the reservation guards the write side, so it
        // has to fall in the same transaction as the event that empties the seat.
        await reservations.ReleaseAsync(aggregate.OrganizationId, command.UserId, ct);
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

        // No mail, but the membership notice still goes out: losing a team is not news to tell
        // somebody, it is a fact other modules keep a copy of.
        var messages = new OutgoingMessages
        {
            TeamMembershipChanged.NoTeam(
                command.UserId,
                aggregate.OrganizationId.Value,
                @event.OccurredAt
            ),
        };

        return (@event, [@event], messages);
    }
}
