using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.LegalEntities.Application.Port;

public interface ILegalEntitiesQueryRepository
{
    Task<SliceResponse<LegalEntityProjection>> GetLegalEntities(
        OrganizationId organizationId,
        string search,
        bool activeOnly,
        DateOnly today,
        int page,
        int pageSize,
        CancellationToken ct
    );

    Task<LegalEntityProjection?> GetLegalEntity(
        OrganizationId organizationId,
        Guid legalEntityId,
        CancellationToken ct
    );
}
