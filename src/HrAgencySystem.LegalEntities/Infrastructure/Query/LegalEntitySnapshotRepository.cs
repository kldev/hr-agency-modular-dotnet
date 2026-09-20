using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.LegalEntities.Infrastructure.Query;

public sealed class LegalEntitySnapshotRepository(IDocumentSession session)
    : ILegalEntitySnapshotRepository
{
    public async Task<LegalEntitySnapshot?> GetLegalEntityAsync(
        Guid legalEntityId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<LegalEntityProjection>()
            .Where(z => z.Id == legalEntityId && z.OrganizationId == organizationId.Value)
            .Select(z => new LegalEntitySnapshot(
                z.Id,
                z.Name,
                z.LegalName,
                z.TaxId,
                z.VatNumber,
                z.RegisteredAddress,
                z.ActiveFrom,
                z.ActiveTo
            ))
            .FirstOrDefaultAsync(ct);

        if (result is not null)
            return result;

        // The projection runs in the async daemon, and a project is often created straight after
        // the entity that will deliver it. Falling back to the stream keeps that from failing over
        // a read model that is a second behind - the same fallback CompanySnapshotRepository makes.
        return await session
            .Events.QueryRawEventDataOnly<LegalEntityCreated>()
            .Where(z =>
                z.LegalEntityId == legalEntityId && z.OrganizationId == organizationId.Value
            )
            .Select(z => new LegalEntitySnapshot(
                z.LegalEntityId,
                z.Name,
                z.LegalName,
                z.TaxId,
                z.VatNumber,
                z.RegisteredAddress,
                z.ActiveFrom,
                z.ActiveTo
            ))
            .FirstOrDefaultAsync(ct);
    }
}
