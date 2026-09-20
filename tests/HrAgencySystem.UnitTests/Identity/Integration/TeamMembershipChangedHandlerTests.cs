using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Integration;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Wolverine;

namespace HrAgencySystem.UnitTests.Identity.Integration;

public sealed class TeamMembershipChangedHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly Guid TeamId = Guid.NewGuid();

    private static readonly DateTimeOffset Now = new(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WithATeam_RestatesItAsAnIdentityEvent()
    {
        var captured = await Handle(
            new TeamMembershipChanged(UserId, OrgId, TeamId, "Tiggers", TeamRole.Recruiter, Now)
        );

        Assert.Equal(UserId, captured.UserId);
        Assert.Equal(OrgId, captured.OrganizationId);
        Assert.Equal(Now, captured.ChangedAt);
        Assert.NotNull(captured.Team);
        Assert.Equal(TeamId, captured.Team.Id);
        Assert.Equal("Tiggers", captured.Team.Name);
        Assert.Equal(TeamRole.Recruiter, captured.Team.Role);
    }

    [Fact]
    public async Task HandleAsync_WithoutATeam_ClearsTheMembership()
    {
        var captured = await Handle(
            new TeamMembershipChanged(UserId, OrgId, null, null, null, Now)
        );

        Assert.Equal(UserId, captured.UserId);
        Assert.Null(captured.Team);
    }

    private static async Task<UserTeamChanged> Handle(TeamMembershipChanged message)
    {
        var bus = Substitute.For<IMessageBus>();
        var sent = new List<object>();

        bus.InvokeAsync(
                Arg.Do<object>(sent.Add),
                Arg.Any<CancellationToken>(),
                Arg.Any<TimeSpan?>()
            )
            .Returns(Task.CompletedTask);

        await new TeamMembershipChangedHandler().HandleAsync(
            message,
            bus,
            NullLogger<TeamMembershipChanged>.Instance
        );

        return Assert.IsType<UserTeamChanged>(Assert.Single(sent));
    }
}
