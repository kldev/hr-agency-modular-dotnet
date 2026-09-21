using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Services;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Workers;

/// <summary>
/// The module's one way out, tested for real rather than through a substitute - these are the
/// refusals that keep one agency's register out of another's, so a mock standing in for them would
/// prove nothing.
/// </summary>
public class WorkersServiceTests
{
    private readonly IUserSnapshotRepository _users = Substitute.For<IUserSnapshotRepository>();
    private readonly IProjectSnapshotRepository _projects =
        Substitute.For<IProjectSnapshotRepository>();
    private readonly IPositionSnapshotRepository _positions =
        Substitute.For<IPositionSnapshotRepository>();
    private readonly IOrganizationChecker _checker = Substitute.For<IOrganizationChecker>();

    private WorkersService Service() =>
        new(_users, _projects, _positions, _checker, Substitute.For<IWorkerRepository>());

    [Fact]
    public void ValidateAggregateUpdate_ForAnotherOrganization_Refuses()
    {
        var worker = WorkerScenario.Registered(organizationId: Guid.NewGuid());

        Assert.Throws<OrganizationAccessDeniedException>(() =>
            Service().ValidateAggregateUpdate(worker, WorkerScenario.OrganizationId)
        );
    }

    [Fact]
    public void ValidateAggregateUpdate_ForAMissingAggregate_Refuses()
    {
        Assert.Throws<OrganizationAccessDeniedException>(() =>
            Service().ValidateAggregateUpdate(null!, WorkerScenario.OrganizationId)
        );
    }

    [Fact]
    public async Task GetProjectAsync_ForAProjectInAnotherOrganization_IsABusinessRuleNotA404()
    {
        // A 404 here would confirm that the id exists in somebody else's tenant.
        _projects
            .GetProjectAsync(
                Arg.Any<Guid>(),
                Arg.Any<OrganizationId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((ProjectSnapshot?)null);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Service()
                .GetProjectAsync(
                    OrganizationId.From(WorkerScenario.OrganizationId),
                    WorkerScenario.ProjectId,
                    CancellationToken.None
                )
        );

        Assert.Equal(IWorkersService.ProjectNotInOrganizationMessage, error.Message);
    }

    [Fact]
    public async Task GetProjectAsync_ForAFinishedProject_Refuses()
    {
        _projects
            .GetProjectAsync(
                Arg.Any<Guid>(),
                Arg.Any<OrganizationId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(WorkerScenario.Project(open: false));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Service()
                .GetProjectAsync(
                    OrganizationId.From(WorkerScenario.OrganizationId),
                    WorkerScenario.ProjectId,
                    CancellationToken.None
                )
        );

        Assert.Equal(IWorkersService.ProjectClosedMessage, error.Message);
    }

    [Fact]
    public async Task GetPositionAsync_ForARoleTheProjectDoesNotHave_IsABusinessRuleNotA404()
    {
        // The same answer for an id that never existed, one from another delivery and one from
        // another agency - telling them apart would say which ids exist elsewhere.
        _positions
            .GetPositionAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<OrganizationId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((PositionSnapshot?)null);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Service()
                .GetPositionAsync(
                    OrganizationId.From(WorkerScenario.OrganizationId),
                    WorkerScenario.ProjectId,
                    WorkerScenario.PositionId,
                    CancellationToken.None
                )
        );

        Assert.Equal(IWorkersService.PositionNotOnProjectMessage, error.Message);
    }

    /// <summary>
    /// A role is archived when it has run its course, and the people on it work out their notice.
    /// What stops is putting somebody new on it.
    /// </summary>
    [Fact]
    public async Task GetPositionAsync_ForAnArchivedRole_Refuses()
    {
        _positions
            .GetPositionAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<OrganizationId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(WorkerScenario.Position(archived: true));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Service()
                .GetPositionAsync(
                    OrganizationId.From(WorkerScenario.OrganizationId),
                    WorkerScenario.ProjectId,
                    WorkerScenario.PositionId,
                    CancellationToken.None
                )
        );

        Assert.Equal(IWorkersService.PositionArchivedMessage, error.Message);
    }

    [Fact]
    public async Task GetUserAsync_ForAnUnknownUser_IsA404()
    {
        _users
            .GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service().GetUserAsync(WorkerScenario.UserId, CancellationToken.None)
        );
    }
}
