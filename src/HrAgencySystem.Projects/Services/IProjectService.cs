using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Projects.Services;

/// <summary>
/// The module's one way out to everybody else. Handlers talk to this, never straight to the shared
/// kernel ports, so "what does a project need to know about other modules" has a single answer.
/// </summary>
public interface IProjectService
{
    public const string CompanyNotInOrganizationMessage =
        "The specified company does not exist in this organization.";

    public const string TeamNotInOrganizationMessage =
        "The specified team does not exist in this organization.";

    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);

    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    /// <summary>
    /// Resolves a company that must belong to the given organization. Fails with a business rule
    /// rather than a 404, so the answer never reveals whether the id exists in somebody else's
    /// tenant.
    /// </summary>
    Task<CompanySnapshot> GetCompanyAsync(
        OrganizationId organizationId,
        Guid companyId,
        CancellationToken ct
    );

    Task<TeamSnapshot> GetTeamAsync(
        OrganizationId organizationId,
        Guid teamId,
        CancellationToken ct
    );

    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
}
