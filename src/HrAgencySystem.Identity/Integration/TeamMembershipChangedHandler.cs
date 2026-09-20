using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace HrAgencySystem.Identity.Integration;

[WolverineHandler]
public class TeamMembershipChangedHandler
{
    public async Task HandleAsync(
        TeamMembershipChanged message,
        IMessageBus bus,
        ILogger<TeamMembershipChanged> logger
    )
    {
        logger.LogInformation(
            "Handling TeamMembershipChanged for user {UserId}, team {TeamId}",
            message.UserId,
            message.TeamId
        );

        // Team, name and role travel together or not at all — Teams sends all three or sends none,
        // so there is no half-filled case to guard against beyond this one check.
        var team = message.TeamId is null
            ? null
            : new TeamInfo(message.TeamId.Value, message.TeamName!, message.Role!.Value);

        var @event = new UserTeamChanged(
            message.UserId,
            message.OrganizationId,
            team,
            message.OccurredAt
        );

        await bus.InvokeAsync(@event);
    }
}

public static class UserTeamChangedHandler
{
    [AggregateHandler]
    public static Task<UserTeamChanged> Handle(UserTeamChanged command, User aggregate)
    {
        return Task.FromResult(command);
    }
}
