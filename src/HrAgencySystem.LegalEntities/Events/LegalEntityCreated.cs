using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.LegalEntities.Events;

public sealed record LegalEntityCreated(
    Guid LegalEntityId,
    Guid OrganizationId,
    string Name,
    string LegalName,
    string TaxId,
    string? VatNumber,
    PostalAddress RegisteredAddress,
    string Description,
    President President,
    IReadOnlyList<LegalEntityBankAccount> BankAccounts,
    DateOnly ActiveFrom,
    DateOnly? ActiveTo,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);
