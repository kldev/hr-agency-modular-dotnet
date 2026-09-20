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

    public const string LegalEntityNotInOrganizationMessage =
        "The specified legal entity does not exist in this organization.";

    public const string LegalEntityNotTradingMessage =
        "That legal entity was no longer trading when the project starts.";

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

    /// <summary>
    /// Resolves one of our own companies, which must belong to the caller's organization and must
    /// still have been trading on the day the project starts - an engagement cannot be run by a
    /// company that was already wound up.
    /// </summary>
    Task<LegalEntitySnapshot> GetLegalEntityAsync(
        OrganizationId organizationId,
        Guid legalEntityId,
        DateOnly startsOn,
        CancellationToken ct
    );

    Task<TeamSnapshot> GetTeamAsync(
        OrganizationId organizationId,
        Guid teamId,
        CancellationToken ct
    );

    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
}
