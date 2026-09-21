using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Services;

/// <summary>
/// The module's one way out. Everything it needs from elsewhere is a user, which it reads through
/// the SharedKernel port rather than by referencing Identity.
/// </summary>
public interface IAgencyService
{
    public const string MemberNotInOrganizationMessage =
        "The specified person is not a member of this organization.";

    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Resolves a user that must belong to the given organization. A business rule rather than a
    /// 404, so the answer never says whether the id exists in somebody else's tenant.
    /// </summary>
    Task<UserSnapshot> GetOrganizationMemberAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    );

    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
}
