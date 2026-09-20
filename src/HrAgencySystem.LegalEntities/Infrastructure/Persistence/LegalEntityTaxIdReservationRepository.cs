using HrAgencySystem.LegalEntities.Application.Port;
using HrAgencySystem.LegalEntities.Domain;
using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.LegalEntities.Infrastructure.Persistence;

public sealed class LegalEntityTaxIdReservationRepository(IDocumentSession session)
    : ILegalEntityTaxIdReservationRepository
{
    public async Task<bool> ExistsAsync(
        OrganizationId organizationId,
        TaxId taxId,
        CancellationToken ct
    )
    {
        return await session
            .Query<LegalEntityTaxIdReservation>()
            .Where(z => z.OrganizationId == organizationId.Value && z.TaxId == taxId.Value)
            .AnyAsync(ct);
    }

    public Task ReserveAsync(
        OrganizationId organizationId,
        TaxId taxId,
        LegalEntityId legalEntityId
    )
    {
        session.Insert(
            new LegalEntityTaxIdReservation(
                Guid.NewGuid(),
                organizationId.Value,
                legalEntityId.Value,
                taxId.Value
            )
        );

        return Task.CompletedTask;
    }

    public async Task ChangeTaxIdAsync(
        OrganizationId organizationId,
        LegalEntityId legalEntityId,
        TaxId taxId,
        CancellationToken ct
    )
    {
        var reservation = await session
            .Query<LegalEntityTaxIdReservation>()
            .Where(z =>
                z.OrganizationId == organizationId.Value && z.LegalEntityId == legalEntityId.Value
            )
            .SingleOrDefaultAsync(ct);

        if (reservation is null)
            throw new NotFoundException("Legal entity tax id reservation", legalEntityId.Value);

        if (reservation.TaxId == taxId.Value)
            return;

        session.Update(reservation with { TaxId = taxId.Value });
    }
}
