using HrAgencySystem.LegalEntities.Domain;
using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.LegalEntities.Application.Port;

/// <summary>
/// Two of our own entities cannot share a tax number inside one organization - it is the same
/// company entered twice. Enforced by a reservation document with a unique index, the way every
/// other cross-aggregate uniqueness rule here is.
/// </summary>
public interface ILegalEntityTaxIdReservationRepository
{
    public const string AlreadyUsedMessage = "A legal entity with this tax ID already exists.";

    Task<bool> ExistsAsync(OrganizationId organizationId, TaxId taxId, CancellationToken ct);

    Task ReserveAsync(OrganizationId organizationId, TaxId taxId, LegalEntityId legalEntityId);

    Task ChangeTaxIdAsync(
        OrganizationId organizationId,
        LegalEntityId legalEntityId,
        TaxId taxId,
        CancellationToken ct
    );
}
