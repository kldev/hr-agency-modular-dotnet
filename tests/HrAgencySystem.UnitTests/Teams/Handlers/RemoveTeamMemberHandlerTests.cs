using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Members.Remove;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Services;
using NSubstitute;
using TeamMemberRemoved = HrAgencySystem.Teams.Events.TeamMemberRemoved;
using TeamMemberSnapshot = HrAgencySystem.Teams.Events.TeamMemberSnapshot;

namespace HrAgencySystem.UnitTests.Teams.Handlers;

public sealed class RemoveTeamMemberHandlerTests
{
    private readonly ITeamsService _service = Substitute.For<ITeamsService>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public RemoveTeamMemberHandlerTests()
    {
        _clock.UtcNow.Returns(TeamsTestData.Now);
        Know(TeamsTestData.SalesUser);
        Know(TeamsTestData.RecruiterUser);
        Know(TeamsTestData.Actor);
    }

    [Fact]
    public async Task Handle_WithTwoMembers_RemovesTheOneAskedFor()
    {
        var (@event, _) = await Handle(TeamsTestData.RecruiterUser.Id);

        Assert.Equal(TeamsTestData.TeamStreamId, @event.TeamId);
        Assert.Equal(TeamsTestData.RecruiterUser, @event.Member.User);
        // The role comes from the aggregate, not from the command — the caller never states it.
        Assert.Equal(TeamRole.Recruiter, @event.Member.Role);
        Assert.Equal(TeamsTestData.Actor, @event.RemovedBy);
    }

    [Fact]
    public async Task Handle_WithTheLastMember_Throws()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                TeamsTestData.SalesUser.Id,
                new TeamMemberSnapshot(TeamsTestData.SalesUser, TeamRole.Sales)
            )
        );

        Assert.Equal(RemoveTeamMemberHandler.LastMemberMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithSomebodyNotOnTheTeam_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Handle(Guid.NewGuid()));
    }

    private void Know(UserSnapshot user)
    {
        _service.GetUserAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
    }

    private async Task<(TeamMemberRemoved, Wolverine.Marten.Events)> Handle(
        Guid userId,
        params TeamMemberSnapshot[] roster
    )
    {
        TeamMemberSnapshot[] members =
            roster.Length > 0
                ? roster
                :
                [
                    new TeamMemberSnapshot(TeamsTestData.SalesUser, TeamRole.Sales),
                    new TeamMemberSnapshot(TeamsTestData.RecruiterUser, TeamRole.Recruiter),
                ];

        var command = new RemoveTeamMember(
            TeamsTestData.TeamStreamId,
            TeamsTestData.OrgId,
            userId,
            TeamsTestData.Actor.Id
        );

        return await RemoveTeamMemberHandler.Handle(
            command,
            TeamsTestData.CreateAggregate(members),
            _service,
            _clock,
            CancellationToken.None
        );
    }
}
