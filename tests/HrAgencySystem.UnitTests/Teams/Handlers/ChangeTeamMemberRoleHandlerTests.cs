using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Members.ChangeRole;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using HrAgencySystem.Teams.Services;
using NSubstitute;
using Wolverine;
using TeamMemberRoleChanged = HrAgencySystem.Teams.Events.TeamMemberRoleChanged;
using TeamMemberSnapshot = HrAgencySystem.Teams.Events.TeamMemberSnapshot;

namespace HrAgencySystem.UnitTests.Teams.Handlers;

public sealed class ChangeTeamMemberRoleHandlerTests
{
    private readonly ITeamsService _service = Substitute.For<ITeamsService>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public ChangeTeamMemberRoleHandlerTests()
    {
        _clock.UtcNow.Returns(TeamsTestData.Now);
    }

    [Fact]
    public async Task Handle_WhenSomebodyElseChangesYourRole_SendsNotificationWithBothRoles()
    {
        var (@event, _, messages) = await Handle(
            TeamsTestData.RecruiterUser,
            TeamsTestData.Actor,
            TeamRole.Lead
        );

        Assert.Equal(TeamRole.Lead, @event.Member.Role);
        Assert.Equal(TeamRole.Recruiter, @event.PreviousRole);

        var notification = Assert.Single(messages.OfType<SendTeamMemberRoleChanged>());
        Assert.Equal(nameof(TeamRole.Lead), notification.Role);
        Assert.Equal(nameof(TeamRole.Recruiter), notification.PreviousRole);
        Assert.Equal(TeamsTestData.RecruiterUser.Email, notification.MemberEmail);
        Assert.Equal(TeamsTestData.Actor.Fullname, notification.ChangedByFullname);
    }

    [Fact]
    public async Task Handle_WhenYouChangeYourOwnRole_SendsNoNotification()
    {
        var (_, _, messages) = await Handle(
            TeamsTestData.RecruiterUser,
            TeamsTestData.RecruiterUser,
            TeamRole.Lead
        );

        // The membership notice always goes out — it is how other modules keep their copy. The mail
        // is what changing your own role suppresses.
        Assert.Empty(messages.OfType<SendTeamMemberRoleChanged>());
        Assert.Single(messages.OfType<TeamMembershipChanged>());
    }

    [Fact]
    public async Task Handle_WithTheRoleTheyAlreadyHold_Throws()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(TeamsTestData.RecruiterUser, TeamsTestData.Actor, TeamRole.Recruiter)
        );

        Assert.Equal(ChangeTeamMemberRoleHandler.SameRoleMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithSomebodyNotOnTheTeam_Throws()
    {
        var outsider = new UserSnapshot(Guid.NewGuid(), "Eve", "Stranger", "eve@hr-agency.com");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Handle(outsider, TeamsTestData.Actor, TeamRole.Lead)
        );
    }

    private async Task<(TeamMemberRoleChanged, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        UserSnapshot member,
        UserSnapshot changedBy,
        TeamRole role
    )
    {
        var command = new ChangeTeamMemberRole(
            TeamsTestData.TeamStreamId,
            TeamsTestData.OrgId,
            member.Id,
            role,
            changedBy.Id
        );

        _service
            .GetOrganizationMemberAsync(
                Arg.Any<OrganizationId>(),
                member.Id,
                Arg.Any<CancellationToken>()
            )
            .Returns(member);
        _service.GetUserAsync(changedBy.Id, Arg.Any<CancellationToken>()).Returns(changedBy);

        return await ChangeTeamMemberRoleHandler.Handle(
            command,
            TeamsTestData.CreateAggregate(
                new TeamMemberSnapshot(TeamsTestData.SalesUser, TeamRole.Sales),
                new TeamMemberSnapshot(TeamsTestData.RecruiterUser, TeamRole.Recruiter)
            ),
            _service,
            _clock,
            CancellationToken.None
        );
    }
}
