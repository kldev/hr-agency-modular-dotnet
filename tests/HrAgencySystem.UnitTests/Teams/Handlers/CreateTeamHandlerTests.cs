using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Create;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Services;
using Marten;
using NSubstitute;
using TeamCreated = HrAgencySystem.Teams.Events.TeamCreated;

namespace HrAgencySystem.UnitTests.Teams.Handlers;

public sealed class CreateTeamHandlerTests
{
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly ITeamsService _service = Substitute.For<ITeamsService>();
    private readonly ITeamMembershipReservationRepository _reservations =
        Substitute.For<ITeamMembershipReservationRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public CreateTeamHandlerTests()
    {
        _clock.UtcNow.Returns(TeamsTestData.Now);
        NobodyIsOnATeam();
        _service
            .GetUserAsync(TeamsTestData.Actor.Id, Arg.Any<CancellationToken>())
            .Returns(TeamsTestData.Actor);
        KnowMember(TeamsTestData.SalesUser);
        KnowMember(TeamsTestData.RecruiterUser);
        KnowMember(TeamsTestData.OpsUser);
    }

    [Fact]
    public async Task Handle_WithFullRoster_ReturnsTeamCreatedWithEveryMember()
    {
        var (@event, _) = await Handle(
            new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Sales),
            new CreateTeamMember(TeamsTestData.RecruiterUser.Id, TeamRole.Recruiter),
            new CreateTeamMember(TeamsTestData.OpsUser.Id, TeamRole.Operations)
        );

        Assert.Equal(TeamsTestData.OrgId, @event.OrganizationId);
        Assert.Equal("ShawSzenk", @event.Name);
        Assert.Equal(TeamsTestData.Actor, @event.CreatedBy);
        Assert.Equal(TeamsTestData.Now, @event.CreatedAt);
        Assert.Equal(3, @event.Members.Count);
        Assert.Equal(
            [TeamRole.Sales, TeamRole.Recruiter, TeamRole.Operations],
            @event.Members.Select(m => m.Role)
        );
        Assert.Equal(TeamsTestData.RecruiterUser.Email, @event.Members[1].User.Email);
    }

    [Fact]
    public async Task Handle_TrimsTheName()
    {
        var (@event, _) = await Handle(
            "   Tiggers  ",
            new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Sales)
        );

        Assert.Equal("Tiggers", @event.Name);
    }

    [Fact]
    public async Task Handle_StartsTheStream()
    {
        await Handle(new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Sales));

        _session.Events.Received(1).StartStream<Team>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithNoMembers_Throws()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle());

        Assert.Equal(CreateTeamHandler.EmptyMembersMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithTheSamePersonTwice_Throws()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Sales),
                new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Lead)
            )
        );

        Assert.Equal(CreateTeamHandler.DuplicateMemberMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithBlankName_Throws()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Handle("  ", new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Sales))
        );
    }

    [Fact]
    public async Task Handle_WithMemberFromAnotherOrganization_Throws()
    {
        var outsider = new UserSnapshot(Guid.NewGuid(), "Eve", "Stranger", "eve@other.com");

        _service
            .GetOrganizationMemberAsync(
                Arg.Any<OrganizationId>(),
                outsider.Id,
                Arg.Any<CancellationToken>()
            )
            .Returns<UserSnapshot>(_ =>
                throw new BusinessRuleException(ITeamsService.MemberNotInOrganizationMessage)
            );

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(new CreateTeamMember(outsider.Id, TeamRole.Sales))
        );

        Assert.Equal(ITeamsService.MemberNotInOrganizationMessage, error.Message);
    }

    [Fact]
    public async Task Handle_ReservesEveryMember()
    {
        await Handle(
            new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Sales),
            new CreateTeamMember(TeamsTestData.RecruiterUser.Id, TeamRole.Recruiter)
        );

        await _reservations
            .Received(1)
            .ReserveAsync(Arg.Any<OrganizationId>(), TeamsTestData.SalesUser.Id, Arg.Any<Guid>());
        await _reservations
            .Received(1)
            .ReserveAsync(
                Arg.Any<OrganizationId>(),
                TeamsTestData.RecruiterUser.Id,
                Arg.Any<Guid>()
            );
    }

    [Fact]
    public async Task Handle_WithSomebodyAlreadyOnAnotherTeam_Throws()
    {
        _reservations
            .FindAssignedAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns<IReadOnlyList<Guid>>([TeamsTestData.SalesUser.Id]);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(new CreateTeamMember(TeamsTestData.SalesUser.Id, TeamRole.Sales))
        );

        Assert.Equal(ITeamMembershipReservationRepository.AlreadyOnTeamMessage, error.Message);
    }

    private void NobodyIsOnATeam()
    {
        _reservations
            .FindAssignedAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns<IReadOnlyList<Guid>>([]);
    }

    private void KnowMember(UserSnapshot user)
    {
        _service
            .GetOrganizationMemberAsync(
                Arg.Any<OrganizationId>(),
                user.Id,
                Arg.Any<CancellationToken>()
            )
            .Returns(user);
    }

    private Task<(TeamCreated Event, Wolverine.OutgoingMessages Messages)> Handle(
        params CreateTeamMember[] members
    ) => Handle("ShawSzenk", members);

    private async Task<(TeamCreated Event, Wolverine.OutgoingMessages Messages)> Handle(
        string name,
        params CreateTeamMember[] members
    )
    {
        var command = new CreateTeam(TeamsTestData.OrgId, name, members, TeamsTestData.Actor.Id);

        return await CreateTeamHandler.Handle(
            command,
            _session,
            _service,
            _reservations,
            _clock,
            CancellationToken.None
        );
    }
}
