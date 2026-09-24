using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Members.Add;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using HrAgencySystem.Teams.Services;
using NSubstitute;
using Wolverine;
using TeamMemberAdded = HrAgencySystem.Teams.Events.TeamMemberAdded;
using TeamMemberSnapshot = HrAgencySystem.Teams.Events.TeamMemberSnapshot;

namespace HrAgencySystem.UnitTests.Teams.Handlers;

public sealed class AddTeamMemberHandlerTests
{
    private readonly ITeamsService _service = Substitute.For<ITeamsService>();
    private readonly ITeamMembershipReservationRepository _reservations =
        Substitute.For<ITeamMembershipReservationRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private bool _alreadyOnATeam;

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

        // The membership notice always goes out — it is how other modules keep their copy. The mail
        // is what self-assignment suppresses.
        Assert.Empty(messages.OfType<SendTeamMemberAdded>());
        Assert.Single(messages.OfType<TeamMembershipChanged>());
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

    [Fact]
    public async Task Handle_WithSomebodyAlreadyOnAnotherTeam_Throws()
    {
        _alreadyOnATeam = true;

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(TeamsTestData.RecruiterUser, TeamsTestData.Actor)
        );

        Assert.Equal(ITeamMembershipReservationRepository.AlreadyOnTeamMessage, error.Message);
    }

    [Fact]
    public async Task Handle_ReservesTheSeat()
    {
        await Handle(TeamsTestData.RecruiterUser, TeamsTestData.Actor);

        await _reservations
            .Received(1)
            .ReserveAsync(
                Arg.Any<OrganizationId>(),
                TeamsTestData.RecruiterUser.Id,
                TeamsTestData.TeamStreamId
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

        _reservations
            .FindAssignedAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_alreadyOnATeam ? [command.UserId] : []);

        return await AddTeamMemberHandler.Handle(
            command,
            TeamsTestData.CreateAggregate(
                new TeamMemberSnapshot(TeamsTestData.SalesUser, TeamRole.Sales)
            ),
            _service,
            _reservations,
            _clock,
            CancellationToken.None
        );
    }
}
