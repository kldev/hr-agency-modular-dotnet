using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Members.Add;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Services;
using NSubstitute;
using Wolverine;
using TeamMemberAdded = HrAgencySystem.Teams.Events.TeamMemberAdded;
using TeamMemberSnapshot = HrAgencySystem.Teams.Events.TeamMemberSnapshot;

namespace HrAgencySystem.UnitTests.Teams.Handlers;

public sealed class AddTeamMemberHandlerTests
{
    private readonly ITeamsService _service = Substitute.For<ITeamsService>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public AddTeamMemberHandlerTests()
    {
        _clock.UtcNow.Returns(TeamsTestData.Now);
    }

    [Fact]
    public async Task Handle_WhenSomebodyElseAddsYou_SendsNotification()
    {
        var (@event, _, messages) = await Handle(TeamsTestData.RecruiterUser, TeamsTestData.Actor);

        Assert.Equal(TeamsTestData.TeamStreamId, @event.TeamId);
        Assert.Equal(TeamRole.Recruiter, @event.Member.Role);
        Assert.Equal(TeamsTestData.RecruiterUser, @event.Member.User);

        var notification = Assert.Single(messages.OfType<SendTeamMemberAdded>());
        Assert.Equal(TeamsTestData.TeamStreamId, notification.TeamId);
        Assert.Equal(TeamsTestData.Name, notification.TeamName);
        Assert.Equal(TeamsTestData.RecruiterUser.Email, notification.MemberEmail);
        Assert.Equal(TeamsTestData.RecruiterUser.Fullname, notification.MemberFullname);
        Assert.Equal(nameof(TeamRole.Recruiter), notification.Role);
        Assert.Equal(TeamsTestData.Actor.Fullname, notification.AddedByFullname);
    }

    [Fact]
    public async Task Handle_WhenYouAddYourself_SendsNoNotification()
    {
        var (_, _, messages) = await Handle(
            TeamsTestData.RecruiterUser,
            TeamsTestData.RecruiterUser
        );

        Assert.Empty(messages);
    }

    [Fact]
    public async Task Handle_WithSomebodyAlreadyOnTheTeam_Throws()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(TeamsTestData.SalesUser, TeamsTestData.Actor)
        );

        Assert.Equal(AddTeamMemberHandler.DuplicateMemberMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithAnotherOrganizationsTeam_Throws()
    {
        _service
            .When(s => s.ValidateAggregateUpdate(Arg.Any<IOrganizationDomain>(), Arg.Any<Guid>()))
            .Do(_ => throw new OrganizationAccessDeniedException());

        await Assert.ThrowsAsync<OrganizationAccessDeniedException>(() =>
            Handle(TeamsTestData.RecruiterUser, TeamsTestData.Actor)
        );
    }

    private async Task<(TeamMemberAdded, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        UserSnapshot member,
        UserSnapshot addedBy
    )
    {
        var command = new AddTeamMember(
            TeamsTestData.TeamStreamId,
            TeamsTestData.OrgId,
            member.Id,
            TeamRole.Recruiter,
            addedBy.Id
        );

        _service
            .GetOrganizationMemberAsync(
                Arg.Any<OrganizationId>(),
                member.Id,
                Arg.Any<CancellationToken>()
            )
            .Returns(member);
        _service.GetUserAsync(addedBy.Id, Arg.Any<CancellationToken>()).Returns(addedBy);

        return await AddTeamMemberHandler.Handle(
            command,
            TeamsTestData.CreateAggregate(
                new TeamMemberSnapshot(TeamsTestData.SalesUser, TeamRole.Sales)
            ),
            _service,
            _clock,
            CancellationToken.None
        );
    }
}
