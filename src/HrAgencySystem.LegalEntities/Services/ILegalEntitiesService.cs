using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.LegalEntities.Services;

public interface ILegalEntitiesService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);

    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
}
