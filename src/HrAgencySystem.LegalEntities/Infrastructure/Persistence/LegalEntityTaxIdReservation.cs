namespace HrAgencySystem.LegalEntities.Infrastructure.Persistence;

public sealed record LegalEntityTaxIdReservation(
    Guid Id,
    Guid OrganizationId,
    Guid LegalEntityId,
    string TaxId
);
