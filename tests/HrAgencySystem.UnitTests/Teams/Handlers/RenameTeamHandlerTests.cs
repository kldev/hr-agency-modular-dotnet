using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Rename;
using HrAgencySystem.Teams.Services;
using NSubstitute;
using TeamRenamed = HrAgencySystem.Teams.Events.TeamRenamed;

namespace HrAgencySystem.UnitTests.Teams.Handlers;

public sealed class RenameTeamHandlerTests
{
    private readonly ITeamsService _service = Substitute.For<ITeamsService>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public RenameTeamHandlerTests()
    {
        _clock.UtcNow.Returns(TeamsTestData.Now);
        _service
            .GetUserAsync(TeamsTestData.Actor.Id, Arg.Any<CancellationToken>())
            .Returns(TeamsTestData.Actor);
    }

    [Fact]
    public async Task Handle_WithANewName_ReturnsTeamRenamed()
    {
        var (@event, _) = await Handle("Tiggers");

        Assert.Equal(TeamsTestData.TeamStreamId, @event.TeamId);
        Assert.Equal(TeamsTestData.OrgId, @event.OrganizationId);
        Assert.Equal("Tiggers", @event.Name);
        Assert.Equal(TeamsTestData.Actor, @event.ModifiedBy);
        Assert.Equal(TeamsTestData.Now, @event.ModifiedAt);
    }

    [Fact]
    public async Task Handle_WithTheSameName_Throws()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(TeamsTestData.Name)
        );

        Assert.Equal(RenameTeamHandler.SameNameMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithBlankName_Throws()
    {
        await Assert.ThrowsAsync<ValidationException>(() => Handle("   "));
    }

    [Fact]
    public async Task Handle_WithAnotherOrganizationsTeam_Throws()
    {
        _service
            .When(s => s.ValidateAggregateUpdate(Arg.Any<IOrganizationDomain>(), Arg.Any<Guid>()))
            .Do(_ => throw new OrganizationAccessDeniedException());

        await Assert.ThrowsAsync<OrganizationAccessDeniedException>(() => Handle("Tiggers"));
    }

    private async Task<(TeamRenamed, Wolverine.Marten.Events)> Handle(string name)
    {
        var command = new RenameTeam(
            TeamsTestData.TeamStreamId,
            TeamsTestData.OrgId,
            name,
            TeamsTestData.Actor.Id
        );

        return await RenameTeamHandler.Handle(
            command,
            TeamsTestData.CreateAggregate(),
            _service,
            _clock,
            CancellationToken.None
        );
    }
}
