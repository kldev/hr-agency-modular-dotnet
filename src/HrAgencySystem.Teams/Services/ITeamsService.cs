using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Teams.Services;

public interface ITeamsService
{
    public const string MemberNotInOrganizationMessage =
        "The specified person is not a member of this organization.";

    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Resolves a user that must belong to the given organization. Fails with a business rule, not
    /// a 404, so the answer never reveals whether the id exists in somebody else's tenant.
    /// </summary>
    Task<UserSnapshot> GetOrganizationMemberAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    );

    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
}
