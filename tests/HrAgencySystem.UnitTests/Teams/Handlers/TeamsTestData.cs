using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using Team = HrAgencySystem.Teams.Domain.Team;
using TeamCreated = HrAgencySystem.Teams.Events.TeamCreated;
using TeamMemberSnapshot = HrAgencySystem.Teams.Events.TeamMemberSnapshot;

namespace HrAgencySystem.UnitTests.Teams.Handlers;

internal static class TeamsTestData
{
    internal const string Name = "ShawSzenk";

    internal static readonly Guid OrgId = Guid.NewGuid();
    internal static readonly Guid TeamStreamId = Guid.NewGuid();

    internal static readonly DateTimeOffset Now = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    internal static readonly UserSnapshot SalesUser = new(
        Guid.NewGuid(),
        "Bob",
        "Wells",
        "bob.wells@hr-agency.com"
    );

    internal static readonly UserSnapshot RecruiterUser = new(
        Guid.NewGuid(),
        "Katy",
        "Wells",
        "katy.wells@hr-agency.com"
    );

    internal static readonly UserSnapshot OpsUser = new(
        Guid.NewGuid(),
        "Ann",
        "Novak",
        "ann.novak@hr-agency.com"
    );

    internal static readonly UserSnapshot Actor = new(
        Guid.NewGuid(),
        "John",
        "Smith",
        "john.smith@hr-agency.com"
    );

    internal static Team CreateAggregate(params TeamMemberSnapshot[] members)
    {
        TeamMemberSnapshot[] roster =
            members.Length > 0 ? members : [new TeamMemberSnapshot(SalesUser, TeamRole.Sales)];

        var aggregate = Team.Empty();
        aggregate.Apply(new TeamCreated(TeamStreamId, OrgId, Name, roster, Actor, Now));

        return aggregate;
    }
}
