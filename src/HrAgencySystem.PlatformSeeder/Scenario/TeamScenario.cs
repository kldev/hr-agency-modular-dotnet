using HrAgencySystem.Teams.Application.Create;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class TeamScenario(IMessageBus bus)
{
    // Agency teams get names with some character — they are said out loud far more often than a
    // team id ever is.
    private static readonly string[] Names =
    [
        "ShawSzenk",
        "Tiggers",
        "Byte Wranglers",
        "Nightcrawlers",
        "The Pipeline",
    ];

    private const int MembersPerTeam = 3;

    internal async Task<IReadOnlyList<Guid>> Create(
        Guid organizationId,
        IReadOnlyList<Guid> userIds,
        Guid createdBy,
        int seedCount = 3
    )
    {
        if (userIds.Count < MembersPerTeam)
            return [];

        // Nobody may sit on two teams, so the number of teams is capped by the people available —
        // the roster below walks the list without wrapping around.
        var count = Math.Min(Math.Min(seedCount, Names.Length), userIds.Count / MembersPerTeam);
        var teamIds = new List<Guid>(count);

        for (var index = 0; index < count; index++)
        {
            var members = BuildRoster(userIds, index);

            var command = new CreateTeam(organizationId, Names[index], members, createdBy);

            var result = await bus.InvokeAsync<TeamCreated>(command);

            teamIds.Add(result.TeamId);
        }

        return teamIds;
    }

    // One seat per role, walking the user list so no two teams share a person. Roles here are team
    // roles, unrelated to whatever OrganizationRole those users hold.
    private static List<CreateTeamMember> BuildRoster(IReadOnlyList<Guid> userIds, int index)
    {
        var offset = index * MembersPerTeam;

        return
        [
            new CreateTeamMember(userIds[offset], TeamRole.Sales),
            new CreateTeamMember(userIds[offset + 1], TeamRole.Recruiter),
            new CreateTeamMember(userIds[offset + 2], TeamRole.Operations),
        ];
    }
}
