using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace HrAgencySystem.UnitTests.Projects;

/// <summary>
/// The tenant and cross-module rules themselves, tested against the real service rather than through
/// a handler - a handler using a substitute could never fail them.
/// </summary>
public class ProjectServiceTests
{
    private static readonly OrganizationId Organization = OrganizationId.From(
        ProjectScenario.OrganizationId
    );

    private readonly IUserSnapshotRepository _users = Substitute.For<IUserSnapshotRepository>();
    private readonly ICompanySnapshotRepository _companies =
        Substitute.For<ICompanySnapshotRepository>();
    private readonly ITeamSnapshotRepository _teams = Substitute.For<ITeamSnapshotRepository>();
    private readonly ILegalEntitySnapshotRepository _legalEntities =
        Substitute.For<ILegalEntitySnapshotRepository>();
    private readonly IOrganizationChecker _checker = Substitute.For<IOrganizationChecker>();

    private ProjectService Service => new(_users, _companies, _teams, _legalEntities, _checker);

    [Fact]
    public void ValidateAggregateUpdate_WithAForeignAggregate_Throws()
    {
        var project = ProjectScenario.Draft(organizationId: Guid.NewGuid());

        Assert.Throws<OrganizationAccessDeniedException>(() =>
            Service.ValidateAggregateUpdate(project, ProjectScenario.OrganizationId)
        );
    }

    [Fact]
    public void ValidateAggregateUpdate_WithOwnAggregate_Passes()
    {
        var project = ProjectScenario.Draft();

        Service.ValidateAggregateUpdate(project, ProjectScenario.OrganizationId);
    }

    [Fact]
    public async Task GetCompanyAsync_ForAnotherOrganization_ThrowsBusinessRuleNotNotFound()
    {
        // A business rule, not a 404: "not found" would tell the caller whether that id exists in
        // somebody else's tenant.
        _companies
            .GetCompanyAsync(
                Arg.Any<Guid>(),
                Arg.Any<OrganizationId>(),
                Arg.Any<CancellationToken>()
            )
            .ReturnsNull();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Service.GetCompanyAsync(Organization, ProjectScenario.CompanyId, CancellationToken.None)
        );

        Assert.Equal(IProjectService.CompanyNotInOrganizationMessage, error.Message);
    }

    [Fact]
    public async Task GetTeamAsync_ForAnotherOrganization_ThrowsBusinessRule()
    {
        _teams
            .GetTeamAsync(Arg.Any<Guid>(), Arg.Any<OrganizationId>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Service.GetTeamAsync(Organization, ProjectScenario.TeamId, CancellationToken.None)
        );

        Assert.Equal(IProjectService.TeamNotInOrganizationMessage, error.Message);
    }

    [Fact]
    public async Task GetUserAsync_WhenUnknown_ThrowsNotFound()
    {
        _users.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.GetUserAsync(ProjectScenario.UserId, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ValidateOrganization_WhenMissing_ThrowsBusinessRule()
    {
        _checker.Exists(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Service.ValidateOrganization(ProjectScenario.OrganizationId, CancellationToken.None)
        );
    }
}
