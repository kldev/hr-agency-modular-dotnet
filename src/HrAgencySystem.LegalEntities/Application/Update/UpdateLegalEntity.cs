using HrAgencySystem.LegalEntities.Application.Create;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.LegalEntities.Application.Update;

public sealed record UpdateLegalEntity(
    Guid LegalEntityId,
    Guid OrganizationId,
    string Name,
    string LegalName,
    string TaxId,
    string? VatNumber,
    string Street,
    string BuildingNumber,
    string? UnitNumber,
    string PostalCode,
    string City,
    string CountryCode,
    string? Description,
    string PresidentFirstName,
    string PresidentLastName,
    string? PresidentEmail,
    DateOnly ActiveFrom,
    DateOnly? ActiveTo,
    IReadOnlyList<BankAccountData> BankAccounts,
    Guid ModifiedBy
) : IUpdateCommand, ILegalEntityData;
