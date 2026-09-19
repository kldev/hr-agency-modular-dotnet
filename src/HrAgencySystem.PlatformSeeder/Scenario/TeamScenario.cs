using HrAgencySystem.Teams.Application.Create;
using HrAgencySystem.Teams.Domain;
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

    internal async Task<IReadOnlyList<Guid>> Create(
        Guid organizationId,
        IReadOnlyList<Guid> userIds,
        Guid createdBy,
        int seedCount = 3
    )
    {
        if (userIds.Count < 3)
            return [];

        var count = Math.Min(seedCount, Names.Length);
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

    // One seat per role, walking the user list so teams do not all share the same people. Roles
    // here are team roles, unrelated to whatever OrganizationRole those users hold.
    private static List<CreateTeamMember> BuildRoster(IReadOnlyList<Guid> userIds, int index)
    {
        var offset = index * 3;

        return
        [
            new CreateTeamMember(userIds[offset % userIds.Count], TeamRole.Sales),
            new CreateTeamMember(userIds[(offset + 1) % userIds.Count], TeamRole.Recruiter),
            new CreateTeamMember(userIds[(offset + 2) % userIds.Count], TeamRole.Operations),
        ];
    }
}
