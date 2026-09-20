using HrAgencySystem.LegalEntities.Application.Port;
using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.LegalEntities.Infrastructure.Query;

public sealed class LegalEntitiesQueryRepository(IDocumentSession session)
    : ILegalEntitiesQueryRepository
{
    public async Task<SliceResponse<LegalEntityProjection>> GetLegalEntities(
        OrganizationId organizationId,
        string search,
        bool activeOnly,
        DateOnly today,
        int page,
        int pageSize,
        CancellationToken ct
    )
    {
        var query = session
            .Query<LegalEntityProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .WithActiveOnly(activeOnly, today)
            .OrderBy(z => z.Name);

        return await query.ToSlice(page, pageSize, ct);
    }

    public async Task<LegalEntityProjection?> GetLegalEntity(
        OrganizationId organizationId,
        Guid legalEntityId,
        CancellationToken ct
    )
    {
        return await session
            .Query<LegalEntityProjection>()
            .WithOrganizationId(organizationId)
            .WithLegalEntityId(legalEntityId)
            .FirstOrDefaultAsync(ct);
    }
}
