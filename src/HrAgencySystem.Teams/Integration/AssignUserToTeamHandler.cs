using HrAgencySystem.Teams.Application.Members.Add;
using HrAgencySystem.Teams.Contracts.IntegrationCommands;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;

namespace HrAgencySystem.Teams.Integration;

/// <summary>
/// The one way in: another module asks for somebody to be seated, and the request becomes an
/// ordinary command so it passes every rule a request over HTTP would — same organization, no
/// duplicate, nobody on two teams.
/// </summary>
[WolverineHandler]
public class AssignUserToTeamHandler
{
    public async Task HandleAsync(
        AssignUserToTeam message,
        IMessageBus bus,
        ILogger<AssignUserToTeam> logger
    )
    {
        logger.LogInformation(
            "Assigning user {UserId} to team {TeamId}",
            message.UserId,
            message.TeamId
        );

        var command = new AddTeamMember(
            message.TeamId,
            message.OrganizationId,
            message.UserId,
            message.Role,
            message.RequestedBy
        );

        await bus.InvokeAsync(command);
    }
}
